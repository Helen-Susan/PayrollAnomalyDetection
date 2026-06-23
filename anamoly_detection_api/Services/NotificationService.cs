using anamoly_detection_api.Models.DTO;
using Dapper;
using Microsoft.Data.SqlClient;

namespace anamoly_detection_api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _connectionString;

        public NotificationService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;
        }

        public async Task<List<NotificationDto>>
            GetRecentNotificationsAsync()
        {
            using var connection =
                new SqlConnection(_connectionString);

            var query = @"
SELECT TOP 5
    CycleId,Employee_ID,
    Department,
    AnomalyType,
    Severity,
    ProcessedAt
FROM AnomalyProcessingLog
ORDER BY ProcessedAt DESC
          ";

            var result =
                await connection.QueryAsync<NotificationDto>(
                    query);

            return result.ToList();
        }
    }
}
