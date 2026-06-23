using Dapper;
using Kusto.Data;
using Kusto.Data.Net.Client;
using Microsoft.Data.SqlClient;
using anamoly_detection_api.Models.DTO;
namespace anamoly_detection_api.Services


{
    public class FabricAnomalySyncService:IFabricAnomalySyncService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public FabricAnomalySyncService( IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;
        }
        public async Task SyncAnomaliesAsync()
        {
            var anomalies =  await GetAnomaliesAsync();

            using var connection =   new SqlConnection(  _connectionString);

            await connection.OpenAsync();

            foreach (var anomaly in anomalies)
            {
                if (await IsProcessedAsync(
                        connection,
                        anomaly))
                {
                    continue;
                }

                var tableName = await GetPayrollTableAsync( connection,anomaly.CycleId);

                if (string.IsNullOrEmpty(tableName))
                {
                    continue;
                }

                await UpdatePayrollAsync( connection, tableName, anomaly);

                await InsertLogAsync(  connection,anomaly);
            }
        }
        private async Task<List<FabricAnomalyDto>>GetAnomaliesAsync()
        {
            var anomalies =
                new List<FabricAnomalyDto>();

            var queryUri =
                _configuration["FabricKusto:QueryUri"];

            var database =
                _configuration["FabricKusto:Database"];

            var kcsb =
                new KustoConnectionStringBuilder(queryUri)
                    .WithAadUserPromptAuthentication();

            using var client =
                KustoClientFactory
                    .CreateCslQueryProvider(kcsb);

            var reader =
                client.ExecuteQuery(
                    database,
                    @"AnomalyResults
                                    | project
                                        CycleId,
                                        Employee_ID,
                                        Department,
                                        Anomaly_Type,
                                        Severity,
                                        Detected_At",
                    new Kusto.Data.Common.ClientRequestProperties());

            while (reader.Read())
            {
                anomalies.Add(
                    new FabricAnomalyDto
                    {
                        CycleId =
                            reader["CycleId"]?.ToString() ?? "",

                        Employee_ID =
                            reader["Employee_ID"]?.ToString() ?? "",

                        Department =
                            reader["Department"]?.ToString() ?? "",

                        Anomaly_Type =
                            reader["Anomaly_Type"]?.ToString() ?? "",

                        Severity =
                            reader["Severity"]?.ToString() ?? "",

                       Detected_At =
    reader["Detected_At"] is DBNull
        ? DateTime.MinValue
        : (DateTime)reader["Detected_At"],
                    });
            }

            return anomalies;
        }
        private async Task<bool>IsProcessedAsync(SqlConnection connection,FabricAnomalyDto anomaly)
        {
            var exists =
                await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
              FROM AnomalyProcessingLog
              WHERE CycleId = @CycleId
              AND Employee_ID = @EmployeeId
              AND AnomalyType = @AnomalyType",
                    new
                    {
                        anomaly.CycleId,
                        EmployeeId =
                            anomaly.Employee_ID,

                        AnomalyType =
                            anomaly.Anomaly_Type
                    });

            return exists > 0;
        }
        private async Task<string?> GetPayrollTableAsync( SqlConnection connection, string cycleId)
        {
            return await connection
                .ExecuteScalarAsync<string>(
                    @"SELECT TableName
              FROM UploadedFiles
              WHERE Id = @CycleId",
                    new { CycleId = cycleId });
        }
        private async Task UpdatePayrollAsync( SqlConnection connection, string tableName, FabricAnomalyDto anomaly)
        {
            var sql =
                                $"""
                        UPDATE [{tableName}]
                        SET
                            Anomaly_Flag = 'Anomaly',
                            SeverityLevel = @Severity,
                            ReviewStatus = 'Pending Review',
                            AnomalyType = @AnomalyType
                        WHERE Employee_ID = @EmployeeId
                        """;

            await connection.ExecuteAsync(
                sql,
                new
                {
                    anomaly.Severity,

                    AnomalyType =
                        anomaly.Anomaly_Type,

                    EmployeeId =
                        anomaly.Employee_ID
                });
        }
        private async Task InsertLogAsync( SqlConnection connection, FabricAnomalyDto anomaly)
        {
            await connection.ExecuteAsync(
                        @"INSERT INTO
                  AnomalyProcessingLog
                  (
                      CycleId,
                      Employee_ID,
                      AnomalyType,
                      Severity,
                    Department,
                      
                  )
                  VALUES
                  (
                      @CycleId,
                      @EmployeeId,
                      @AnomalyType,
                      @Severity,
@Department
                  )",
                new
                {
                    anomaly.CycleId,

                    EmployeeId =
                        anomaly.Employee_ID,

                    AnomalyType =
                        anomaly.Anomaly_Type,

                    anomaly.Severity
                });
        }
    }
}
