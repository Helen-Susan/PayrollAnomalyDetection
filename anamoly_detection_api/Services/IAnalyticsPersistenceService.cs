using anamoly_detection_api.Models.DTO;

namespace anamoly_detection_api.Services
{
    public interface IAnalyticsPersistenceService
    {
        Task SaveAnalyticsAsync(CycleInformationResponseDto analytics);
    }
}
