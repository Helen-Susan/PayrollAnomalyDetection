using anamoly_detection_api.Models.DTO;

namespace anamoly_detection_api.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetRecentNotificationsAsync();

    }
}
