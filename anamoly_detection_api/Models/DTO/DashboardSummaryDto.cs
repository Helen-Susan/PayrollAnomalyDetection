namespace anamoly_detection_api.Models.DTO
{
    public class DashboardSummaryDto
    {
        public int TotalCycles { get; set; }

        public int TotalEmployees { get; set; }

        public int TotalEmployeesPaid { get; set; }

        public int TotalPendingApprovals { get; set; }

        public int TotalAnomalies { get; set; }

        public List<CycleTrendDto> CycleTrend { get; set; }
            = new();
        public List<DepartmentRiskScoreDto>DepartmentScores
        { get; set; }
        = new();
    }
}