namespace anamoly_detection_api.Models.DTO
{
    public class FabricAnomalyDto
    {
        public string CycleId { get; set; } = string.Empty;

        public string Employee_ID { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Anomaly_Type { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public DateTime Detected_At { get; set; }
    }
}
