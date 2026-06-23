namespace anamoly_detection_api.Models.DTO
{
    public class ExecutiveReportResponseDto
    {
        public string ExecutiveSummary { get; set; }
            = string.Empty;

        public int PayrollHealthScore { get; set; }

        public string RiskLevel { get; set; }
            = string.Empty;

        public List<string> KeyInsights { get; set; }
            = new();

        public List<string> Recommendations { get; set; }
            = new();

        public string Conclusion { get; set; }
            = string.Empty;

        public DashboardSummaryDto DashboardSummary
        { get; set; }
    }
}
