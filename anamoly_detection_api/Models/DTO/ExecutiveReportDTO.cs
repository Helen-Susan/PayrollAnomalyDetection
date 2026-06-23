namespace anamoly_detection_api.Models.DTO
{
    public class ExecutiveReportDto
    {
        public string ExecutiveSummary { get; set; } = string.Empty;

        public int PayrollHealthScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public List<string> KeyInsights { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();

        public List<DepartmentRiskScoreDto> DepartmentRisks { get; set; }
            = new();

        public DashboardSummaryDto DashboardSummary { get; set; }
            = new();
    }
}
