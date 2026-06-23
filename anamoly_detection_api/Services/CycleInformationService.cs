using anamoly_detection_api.Models.DTO;
using Azure;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace anamoly_detection_api.Services
{
    public class CycleInformationService : ICycleInformationService
    {
        #region private variable
        private readonly string _connectionString;
        private readonly IAiCycleInformationService _aiCycleInformationService;
        private readonly IFabricAnomalySyncService _fabricAnomalyService;
        #endregion private variable

        #region constructor
        public CycleInformationService(IConfiguration configuration, IAiCycleInformationService aiCycleInformationService, IFabricAnomalySyncService fabricAnomalyService)
        {
            _connectionString = configuration.GetConnectionString("DefaultSQLConnection")!;
            _aiCycleInformationService = aiCycleInformationService;
            _fabricAnomalyService = fabricAnomalyService;
        }
        #endregion constructor

        #region public methods
        public async Task<CycleInformationResponseDto> GetCycleInformationAsync(string cycleId)
        {
            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            var uploadedFile = await GetUploadedFileAsync(connection, cycleId);

            if (uploadedFile == null)
            {
                return new CycleInformationResponseDto
                {
                    Success = false,
                    Message =
                        $"No payroll cycle found for '{cycleId}'"
                };
            }

            var tableName = uploadedFile.TableName;

            var analytics = new CycleInformationResponseDto();
            Console.WriteLine($"Cycle = {analytics.CycleId}");
            Console.WriteLine($"Anomaly Count = {analytics.Anomalies.Count}");

            analytics.CycleId = cycleId;

            analytics.TableName = tableName;

            analytics.TotalEmployees = await GetTotalEmployeesAsync(connection, tableName);

            analytics.EmployeesPaid = await GetEmployeesPaidAsync(connection, tableName);

            analytics.PendingApprovals = await GetPendingApprovalsAsync(connection, tableName);

            analytics.Anomalies.AddRange(await GetEmptyCalculationAnomaliesAsync(connection, tableName));

            analytics.Anomalies.AddRange(await GetDuplicateEmployeeAnomaliesAsync(connection, tableName));

            analytics.Anomalies.AddRange(await GetNegativeSalaryAnomaliesAsync(connection, tableName));

            analytics.Anomalies.AddRange(await GetHighSalaryAnomaliesAsync(connection, tableName));

            analytics.Anomalies.AddRange(await GetFabricAnomaliesAsync(connection, tableName));

            PopulateTotals(analytics);

            analytics.Success = true;

            analytics.Message =
                "Payroll analytics generated successfully.";

            return analytics;
        }

        public async Task<List<AllCycleHistory>> GetAllUploadedFilesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                    SELECT
                        Id,
                        FileName,
                        Status,
                        UploadedAt
                    FROM UploadedFiles
                    ORDER BY UploadedAt DESC";

            var result = await connection.QueryAsync<AllCycleHistory>(query);

            return result.ToList();
        }

        #endregion public methods

        #region private methods

        private async Task<int> GetTotalEmployeesAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
                SELECT COUNT(*)
                FROM [{tableName}]";

            return await connection
                .ExecuteScalarAsync<int>(
                    query);
        }
        private async Task<int> GetEmployeesPaidAsync(SqlConnection connection, string tableName)
        {
            var query = $@"SELECT COUNT(*)FROM [{tableName}]WHERE LOWER(PaymentStatus) = 'paid'";

            return await connection
                .ExecuteScalarAsync<int>(
                    query);
        }
        private async Task<int> GetPendingApprovalsAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
                            SELECT COUNT(*)
                            FROM [{tableName}]
                            WHERE LOWER(ApprovalStatus) = 'pending'";

            return await connection.ExecuteScalarAsync<int>(
                    query);
        }
        private async Task<List<AnomalyFlagDto>> GetEmptyCalculationAnomaliesAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
                            SELECT
                                Department,Employee_ID,
                                COUNT(*) AS Count
                            FROM [{tableName}]
                            WHERE Net_Pay IS NULL
                               OR Net_Pay = ''
                            GROUP BY Department, Employee_ID";

            var results =
                await connection.QueryAsync<
                    DepartmentAnomalyResult>(
                    query);
            var employees = await connection.QueryAsync<dynamic>(
                                        $@"
                                            SELECT Employee_ID, Department
                                            FROM [{tableName}]
                                            WHERE Net_Pay IS NULL OR Net_Pay = ''
                                                                    GROUP BY Department, Employee_ID");
            var ret = $@" select Id from UploadedFiles where tablename = @tableName";
            var CycleId = await connection.ExecuteScalarAsync<string>(ret, new { tableName });
            foreach (var employee in employees)
            {
                await CreateRuleBasedAnomalyAsync(
                    connection,
                    CycleId,
                    tableName,
                    employee.Employee_ID,
                    employee.Department,
                    "Empty Calculation",
                    "Critical",
                    "Employee has empty net pay.");
            }

            return results.Select(x =>
                new AnomalyFlagDto
                {
                    Department = x.Department,
                    
                    AnomalyType =
                        "Empty Calculation",
                    Count = x.Count,
                    Severity = "Medium",
                    Description =
                        "Missing net pay value detected."
                }).ToList();
        
        }
        private async Task<List<AnomalyFlagDto>> GetDuplicateEmployeeAnomaliesAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
                SELECT
                    duplicates.Department,
                    COUNT(*) AS Count
                FROM
                (
                    SELECT
                        Employee_ID,
                        Department
                    FROM [{tableName}]
                    GROUP BY
                        Employee_ID,
                        Department
                    HAVING COUNT(*) > 1
                ) duplicates
                GROUP BY duplicates.Department";

            var results =
                await connection.QueryAsync<
                    DepartmentAnomalyResult>(
                    query);
            var duplicates = await connection.QueryAsync<dynamic>(
                                            $@"
                                            SELECT Employee_ID, Department
                                            FROM [{tableName}]
                                            GROUP BY Employee_ID, Department
                                            HAVING COUNT(*) > 1");
            var ret = $@" select Id from UploadedFiles where tablename = @tableName";
            var CycleId = await connection.ExecuteScalarAsync<string>(ret, new { tableName });

            foreach (var employee in duplicates)
            {
                await CreateRuleBasedAnomalyAsync(
                    connection,
                    CycleId,
                    tableName,
                    employee.Employee_ID,
                    employee.Department,
                    "Duplicate Employee",
                    "High",
                    "Duplicate employee record detected.");
            }
            return results.Select(x =>
                new AnomalyFlagDto
                {
                    Department = x.Department,
                    AnomalyType =
                        "Duplicate Employee",
                    Count = x.Count,
                    Severity = "High",
                    Description =
                        "Duplicate employee records detected."
                }).ToList();
           
        }
        private async Task<List<AnomalyFlagDto>> GetNegativeSalaryAnomaliesAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
                                SELECT 
                                    Department,
                                    COUNT(*) AS Count
                                FROM [{tableName}]
                                WHERE TRY_CAST(Net_Pay AS FLOAT) < 0
                                GROUP BY Department";

            var results = await connection.QueryAsync<DepartmentAnomalyResult>(query);
            var employees = await connection.QueryAsync<dynamic>(
                                                  $@"
                                                    SELECT Employee_ID, Department
                                                    FROM [{tableName}]
                                                    WHERE TRY_CAST(Net_Pay AS FLOAT) < 0");
            var ret = $@" select Id from UploadedFiles where tablename = @tableName";
            var CycleId = await connection.ExecuteScalarAsync<string>(ret, new { tableName });
            foreach (var employee in employees)
            {
                await CreateRuleBasedAnomalyAsync(
                    connection,
                    CycleId,
                    tableName,
                    employee.Employee_ID,
                    employee.Department,
                    "Negative Salary",
                    "Critical",
                    "Employee has negative net pay.");
            }

            return results.Select(x =>
                new AnomalyFlagDto
                {
                    Department = x.Department,
                    AnomalyType =
                        "Negative Salary",
                    Count = x.Count,
                    Severity = "Critical",
                    Description =
                        "Employees with negative net pay detected."
                }).ToList();
          
        }
        private async Task<List<AnomalyFlagDto>> GetHighSalaryAnomaliesAsync(
      SqlConnection connection,
      string tableName)
        {
            var query = $@"
        SELECT
            Department,
            COUNT(*) AS Count
        FROM [{tableName}]
        WHERE TRY_CAST(Net_Pay AS FLOAT) > 1000000
        GROUP BY Department";

            var results =
                await connection.QueryAsync<DepartmentAnomalyResult>(
                    query);

            var employees =
                await connection.QueryAsync<dynamic>(
                    $@"
            SELECT
                Employee_ID,
                Department
            FROM [{tableName}]
            WHERE TRY_CAST(Net_Pay AS FLOAT) > 1000000");

            var cycleId =
                await connection.ExecuteScalarAsync<string>(
                    @"
            SELECT Id
            FROM UploadedFiles
            WHERE TableName = @TableName",
                    new
                    {
                        TableName = tableName
                    });

            foreach (var employee in employees)
            {
                await CreateRuleBasedAnomalyAsync(
                    connection,
                    cycleId!,
                    tableName,
                    employee.Employee_ID,
                    employee.Department,
                    "High Salary",
                    "Medium",
                    "Salary exceeds configured threshold.");
            }

            return results.Select(x =>
                new AnomalyFlagDto
                {
                    Department = x.Department,
                    AnomalyType = "High Salary",
                    Count = x.Count,
                    Severity = "Medium",
                    Description =
                        "Salary exceeds configured threshold."
                })
                .ToList();
        }
        private void PopulateTotals(CycleInformationResponseDto analytics)
        {
            analytics.TotalAnomalies =
                analytics.Anomalies.Sum(
                    x => x.Count);

            analytics.NegativeSalaries =
                analytics.Anomalies
                    .Where(x =>
                        x.AnomalyType ==
                        "Negative Salary")
                    .Sum(x => x.Count);

            analytics.HighSalaryAnomalies =
                analytics.Anomalies
                    .Where(x =>
                        x.AnomalyType ==
                        "High Salary")
                    .Sum(x => x.Count);

            analytics.EmptyCalculations =
                analytics.Anomalies
                    .Where(x =>
                        x.AnomalyType ==
                        "Empty Calculation")
                    .Sum(x => x.Count);

            analytics.DuplicateEmployees =
                analytics.Anomalies
                    .Where(x =>
                        x.AnomalyType ==
                        "Duplicate Employee")
                    .Sum(x => x.Count);
        }
        private async Task<UploadResponseDto?> GetUploadedFileAsync(SqlConnection connection, string cycleId)
        {
            var query = @"
                            SELECT TOP 1 *
                            FROM UploadedFiles
                            WHERE Id = @cycleId";

            return await connection
                .QueryFirstOrDefaultAsync<UploadResponseDto>(
                    query,
                    new { CycleId = cycleId });
        }
        private async Task<List<AnomalyFlagDto>> GetFabricAnomaliesAsync(SqlConnection connection, string tableName)
        {
            var query = $@"
        SELECT
            Department,
            AnomalyType,
            SeverityLevel,
            COUNT(*) AS Count
        FROM [{tableName}]
        WHERE
            Anomaly_Flag = 'Anomaly'
        GROUP BY
            Department,
            AnomalyType,
            SeverityLevel";

            var results =
                await connection.QueryAsync(
                    query);

            return results.Select(x =>
                new AnomalyFlagDto
                {
                    Department =
                        x.Department,

                    AnomalyType =
                        x.AnomalyType,

                    Count =
                        x.Count,

                    Severity =
                        x.SeverityLevel,

                    Description =
                        $"Detected by AI anomaly engine."
                })
                .ToList();
        }
     private async Task CreateRuleBasedAnomalyAsync( SqlConnection connection, string cycleId, string tableName, string employeeId, string department,
       string anomalyType, string severity, string description)
        {
            // Prevent duplicate logs
            var exists = await connection.ExecuteScalarAsync<int>(
                @"
        SELECT COUNT(*)
        FROM AnomalyProcessingLog
        WHERE CycleId = @CycleId
          AND Employee_ID = @EmployeeId
          AND AnomalyType = @AnomalyType",
                new
                {
                    CycleId = cycleId,
                    EmployeeId = employeeId,
                    AnomalyType = anomalyType
                });

            if (exists > 0)
                return;

            // Update payroll table
            var updatePayroll = $@"
        UPDATE [{tableName}]
        SET
            Anomaly_Flag = 'Anomaly',
            SeverityLevel = @Severity,
            ReviewStatus = 'Pending Review',
            AnomalyType = @AnomalyType
        WHERE Employee_ID = @EmployeeId";

            await connection.ExecuteAsync(
                updatePayroll,
                new
                {
                    EmployeeId = employeeId,
                    Severity = severity,
                    AnomalyType = anomalyType
                });

            // Insert processing log
            await connection.ExecuteAsync(
                @"
        INSERT INTO AnomalyProcessingLog
        (
            CycleId,
            Employee_ID,
            Department,
            AnomalyType,
            Severity,
            ProcessedAt,
            SqlUpdated,
            NotificationSent
        )
        VALUES
        (
            @CycleId,
            @EmployeeId,
            @Department,
            @AnomalyType,
            @Severity,
            GETUTCDATE(),
            1,
            0
        )",
                new
                {
                    CycleId = cycleId,
                    EmployeeId = employeeId,
                    Department = department,
                    AnomalyType = anomalyType,
                    Severity = severity
                });

            // Update cycle status
            await connection.ExecuteAsync(
                @"
        UPDATE UploadedFiles
        SET Status = 'Review Required'
        WHERE Id = @CycleId",
                new
                {
                    CycleId = cycleId
                });
        }
    }
    #endregion private methods
}
