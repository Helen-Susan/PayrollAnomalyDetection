using anamoly_detection_api.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace anamoly_detection_api.Controllers
{
    [ApiController]
    [Route("api/v1/payroll/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService =notificationService;
        }

        [HttpGet]
        public async Task<IActionResult>GetNotifications()
        {
            var notifications =
                await _notificationService
                    .GetRecentNotificationsAsync();

            return Ok(notifications);
        }
    }
}
