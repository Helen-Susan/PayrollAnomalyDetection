using anamoly_detection_api.Services;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/fabric")]
public class FabricTestController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IFabricAnomalySyncService _anomalySyncService;
    private readonly IFabricIngestionService _fabricIngestionService;

    public FabricTestController(IConfiguration configuration, IFabricAnomalySyncService anomalySyncService,IFabricIngestionService  fabricIngestionService)
    {
        _configuration = configuration;
        _anomalySyncService = anomalySyncService;
        _fabricIngestionService = fabricIngestionService;
    }

    [HttpGet("test")]
    public IActionResult TestConnection()
    {
        try
        {
            var queryUri =
                _configuration["FabricKusto:QueryUri"];

            var database =
                _configuration["FabricKusto:Database"];


            var kcsb =
                new KustoConnectionStringBuilder(queryUri)
                    .WithAadUserPromptAuthentication();
           


            using var client =
                KustoClientFactory.CreateCslQueryProvider(kcsb);
            var query = "PayrollRecords | take 1";

            var reader = client.ExecuteQuery(
                database,
                query,
                new ClientRequestProperties());

            

            while (reader.Read())
            {
                return Ok(new
                {
                    Success = true,
                    Employee = reader["Employee_ID"]?.ToString(),
                    Database =
         _configuration["FabricKusto:Database"]
                });
            }

            return Ok(new
            {
                Database =
         _configuration["FabricKusto:DatabaseName"]
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,

                Error = ex.Message,
                Database =
         _configuration["FabricKusto:DatabaseName"]
            });
       
        }
    }
    [HttpPost("sync-anomalies")]
    public async Task<IActionResult> SyncAnomalies()
    {
        await _anomalySyncService
            .SyncAnomaliesAsync();

        return Ok("Sync Completed");
    }
    [HttpPost("test-ingestion/{cycleId}")]
    public async Task<IActionResult> TestIngestion(string cycleId)
    {
        var result = await _fabricIngestionService.IngestPayrollAsync(new List<Dictionary<string, string>>(), cycleId);

        return Ok(new
        {
            Success = result
        });
    }
}