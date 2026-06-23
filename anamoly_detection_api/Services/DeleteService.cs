using Dapper;
using Microsoft.Data.SqlClient;

namespace anamoly_detection_api.Services
{
    public class DeleteCycleService : IDeleteCycleService
    {
        private readonly string _connectionString;

        public DeleteCycleService(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;
        }

        public async Task<bool> DeleteCycleAsync(
            string cycleId)
        {
            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                var getTableQuery = @"
                    SELECT TableName
                    FROM UploadedFiles
                    WHERE Id = @Id";

                var tableName =
                    await connection
                    .QueryFirstOrDefaultAsync<string>(
                        getTableQuery,
                        new { Id = cycleId },
                        transaction);

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    transaction.Rollback();

                    return false;
                }

                var dropTableQuery =
                    $"DROP TABLE [{tableName}]";

                await connection.ExecuteAsync(
                    dropTableQuery,
                    transaction: transaction);

                var deleteFileQuery = @"
                    DELETE FROM UploadedFiles
                    WHERE Id = @Id";

                await connection.ExecuteAsync(
                    deleteFileQuery,
                    new { Id = cycleId },
                    transaction);
        

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
        }
    }
}