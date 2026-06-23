namespace anamoly_detection_api.Models.DTO
{
    public class AnomalyFlagDto
    {
        
        public string AnomalyType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public int Count { get; set; }

        public enum SeverityLevel
        {
            Low,
            Medium,
            High,
            Critical
        }
        public string Severity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}