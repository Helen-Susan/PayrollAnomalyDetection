using Microsoft.Data.SqlClient;

namespace anamoly_detection_api.Models.DTO
{
    public class NotificationDto
    {
        public string CycleId { get; set; } = string.Empty;

        public string Employee_Id { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string AnomalyType { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ProcessedAt { get; set; }
    }
}
