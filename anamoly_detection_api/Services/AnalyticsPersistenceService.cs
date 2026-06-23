using anamoly_detection_api.Models.DTO;
using Dapper;
using Microsoft.Data.SqlClient;

namespace anamoly_detection_api.Services
{
    public class AnalyticsPersistenceService
        : IAnalyticsPersistenceService
    {
        private readonly string _connectionString;

        public AnalyticsPersistenceService( IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;
        }

        public async Task SaveAnalyticsAsync(
            CycleInformationResponseDto analytics)
        {
            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                await InsertPayrollAnalyticsAsync(
                    connection,
                    transaction,
                    analytics);
                Console.WriteLine($"CycleId = {analytics.CycleId}");
                Console.WriteLine($"Anomaly Count = {analytics.Anomalies.Count}");
                await InsertAnomalyFlagsAsync(
                    connection,
                    transaction,
                    analytics);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private async Task InsertPayrollAnalyticsAsync( SqlConnection connection, SqlTransaction transaction, CycleInformationResponseDto analytics)
        {
            var exists =
                await connection.ExecuteScalarAsync<int>(
                @"
        SELECT COUNT(*)
        FROM PayrollAnalytics
        WHERE CycleId = @CycleId",
                new
                {
                    analytics.CycleId
                },
                transaction);

            if (exists > 0)
            {
                return;
            }

            var query = @"
    INSERT INTO PayrollAnalytics
    (
        CycleId,
        TotalEmployees,
        EmployeesPaid,
        PendingApprovals,
        
        TotalAnomalies
    )
    VALUES
    (
        @CycleId,
        @TotalEmployees,
        @EmployeesPaid,
        @PendingApprovals,
       
        @TotalAnomalies
    )";

            await connection.ExecuteAsync(
                query,
                new
                {
                    analytics.CycleId,
                    analytics.TotalEmployees,
                    analytics.EmployeesPaid,
                    analytics.PendingApprovals,
                    
                    analytics.TotalAnomalies,
                },
                transaction);
        }
        private async Task InsertAnomalyFlagsAsync( SqlConnection connection, SqlTransaction transaction, CycleInformationResponseDto analytics)
        {
            await connection.ExecuteAsync(
            @"
    DELETE FROM AnomalyFlags
    WHERE CycleId = @CycleId",
            new
            {
                analytics.CycleId
            },
            transaction);

            var query = @"
    INSERT INTO AnomalyFlags
    (
        CycleId,
        Department,
        AnomalyType,
        Severity,
        Count,
        Description
    )
    VALUES
    (   
        @CycleId,
        @Department,
        @AnomalyType,
        @Severity,
        @Count,
        @Description
    )";

            foreach (var anomaly in analytics.Anomalies)
            {
                await connection.ExecuteAsync(
                    query,
                    new
                    {
                        CycleId =
                            analytics.CycleId,

                        anomaly.Department,
                        anomaly.AnomalyType,
                        anomaly.Severity,
                        anomaly.Count,
                        anomaly.Description
                    },
                    transaction);
            }
        }

    }
}