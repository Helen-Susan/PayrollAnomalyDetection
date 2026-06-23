//this interface to implement ingestion services where the datarows along with payrollname is provided
namespace anamoly_detection_api.Services
{
    public interface IFabricIngestionService
    {
        //function definition 
        Task<bool> IngestPayrollAsync(
      List<Dictionary<string, string>> dataRows,
      string cycleId);
    }
}
