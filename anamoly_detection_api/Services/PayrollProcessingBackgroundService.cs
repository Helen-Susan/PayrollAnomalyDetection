using anamoly_detection_api.Models.DTO;
using Dapper;
using Microsoft.Data.SqlClient;

namespace anamoly_detection_api.Services
{
    public class PayrollProcessingBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PayrollProcessingBackgroundService> _logger;
        private readonly IFabricAnomalySyncService _anomalySyncService; 

        public PayrollProcessingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PayrollProcessingBackgroundService> logger
            )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
          
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Payroll Background Service Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingCyclesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Background Service Error");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }

        private async Task ProcessPendingCyclesAsync()
        {
            using var scope =
                _scopeFactory.CreateScope();

            var configuration =
                scope.ServiceProvider
                    .GetRequiredService<IConfiguration>();

            var connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection");

            using var connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync();

            var cycles =
                await connection.QueryAsync<UploadResponseDto>(
                    @"
                    SELECT *
                    FROM UploadedFiles
                    WHERE Status = 'Pending'
                    ");

            foreach (var cycle in cycles)
            {
                await ProcessCycleAsync(
                    scope.ServiceProvider,
                    connection,
                    cycle);
            }
        }

        private async Task ProcessCycleAsync(  IServiceProvider services, SqlConnection connection, UploadResponseDto cycle)
        {
            try
            {
               

                var anomalySyncService =
                    services.GetRequiredService<
                        IFabricAnomalySyncService>();

                var cycleInformationService =
                    services.GetRequiredService<
                        ICycleInformationService>();

                var analyticsService =
                    services.GetRequiredService<
                        IAnalyticsPersistenceService>();

                _logger.LogInformation(
                    $"Processing Cycle {cycle.Id}");

                // STEP 1 - Upload payroll data to Fabric
               

                // STEP 2 - Sync Fabric anomalies into SQL
                await anomalySyncService
                    .SyncAnomaliesAsync();

                // STEP 3 - Generate Analytics
                var analytics =
                    await cycleInformationService
                        .GetCycleInformationAsync(
                            cycle.Id);

                await analyticsService
                    .SaveAnalyticsAsync(
                        analytics);

                // STEP 4 - Check whether anomalies exist
                var anomalyCount =
                    await connection.ExecuteScalarAsync<int>(
                        @"
                SELECT COUNT(*)
                FROM AnomalyProcessingLog
                WHERE CycleId = @CycleId",
                        new
                        {
                            CycleId = cycle.Id
                        });

                // STEP 5 - Update Status
                var status =
                    anomalyCount > 0
                        ? "Review Required"
                        : "Completed";

                await connection.ExecuteAsync(
                    @"
            UPDATE UploadedFiles
            SET Status = @Status
            WHERE Id = @CycleId",
                    new
                    {
                        Status = status,
                        CycleId = cycle.Fileid
                    });

                _logger.LogInformation(
                    $"Cycle {cycle.Fileid} processed. Status = {status}");
            }
            catch (Exception ex)
            {
                await connection.ExecuteAsync(
                    @"
            UPDATE UploadedFiles
            SET Status = 'Failed'
            WHERE Id = @CycleId",
                    new
                    {
                        CycleId = cycle.Fileid
                    });

                _logger.LogError(
                    ex,
                    $"Cycle {cycle.Fileid} failed.");
            }
        }
    }
}