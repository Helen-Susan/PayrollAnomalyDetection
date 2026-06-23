namespace anamoly_detection_api.Services
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateAnalyticsPdfAsync();
    }
}
