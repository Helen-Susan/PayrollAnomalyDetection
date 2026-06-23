using anamoly_detection_api.Models.DTO;
using anamoly_detection_api.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

public class ReportService : IReportService
{
    private readonly string _connectionString;
    private readonly IRiskScoreService _riskscore;
    public ReportService(IConfiguration configuration,IRiskScoreService riskScoreService)
    {
        _connectionString =
            configuration.GetConnectionString(
                "DefaultSQLConnection")!;
        _riskscore = riskScoreService;
    }

    public async Task<DashboardSummaryDto>
        GetSummaryAsync()
    {
        using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        var dashboard =
            new DashboardSummaryDto();

        var summaryQuery = @"
SELECT
    COUNT(*) AS TotalCycles,

    ISNULL(SUM(TotalEmployees),0)
        AS TotalEmployees,

    ISNULL(SUM(EmployeesPaid),0)
        AS TotalEmployeesPaid,

    ISNULL(SUM(PendingApprovals),0)
        AS TotalPendingApprovals,

    ISNULL(SUM(TotalAnomalies),0)
        AS TotalAnomalies

FROM PayrollAnalytics";

        var summary =
            await connection.QueryFirstOrDefaultAsync<DashboardSummaryDto>(
                summaryQuery);
        if (summary == null)
        {
            return new DashboardSummaryDto
            {
                CycleTrend = new List<CycleTrendDto>()
            };
        }
        var trendQuery = @"
SELECT CycleId,SUM([Count]) AS AnomalyCount FROM AnomalyFlags

GROUP BY CycleId

ORDER BY CycleId";
        var trends =
            await connection.QueryAsync<
                CycleTrendDto>(
                trendQuery);

        var departmentRisks =
         await _riskscore
             .GetDepartmentRiskScoresAsync();
    

        return new DashboardSummaryDto
        {
            TotalCycles =
            summary.TotalCycles,

            TotalEmployees =
            summary.TotalEmployees,

            TotalEmployeesPaid =
            summary.TotalEmployeesPaid,

            TotalPendingApprovals =
            summary.TotalPendingApprovals,

            TotalAnomalies =
            summary.TotalAnomalies,

            CycleTrend =
            trends.ToList(),

            DepartmentScores =
        departmentRisks.ToList()
        };
    }
    
    public async Task<ExecutiveReportDto>GenerateExecutiveReportAsync()
    {
        var dashboard =
            await GetSummaryAsync();

        var report =
            new ExecutiveReportDto();

        report.DashboardSummary = dashboard;

        report.DepartmentRisks =
            dashboard.DepartmentScores;

        report.PayrollHealthScore =
            CalculateHealthScore(dashboard);

        report.RiskLevel =
            GetRiskLevel(report.PayrollHealthScore);

        report.ExecutiveSummary =
            BuildExecutiveSummary(
                dashboard,
                report.RiskLevel);

        report.KeyInsights =
            GenerateInsights(
                dashboard);

        report.Recommendations =
            GenerateRecommendations(
                dashboard);

        return report;
    }
    private int CalculateHealthScore(DashboardSummaryDto dashboard)
    {
        var score = 100;

        score -= dashboard.TotalAnomalies / 5;

        score -= dashboard.TotalPendingApprovals;

        return Math.Max(score, 0);
    }
    private string GetRiskLevel(int score)
    {
        if (score >= 85)
            return "Low";

        if (score >= 70)
            return "Medium";

        if (score >= 50)
            return "High";

        return "Critical";
    }
    private string BuildExecutiveSummary(DashboardSummaryDto dashboard,string riskLevel)
    {
        return $@"
The payroll analytics platform analysed
{dashboard.TotalCycles} payroll cycles
covering {dashboard.TotalEmployees}
employees.

A total of {dashboard.TotalAnomalies}
anomalies were detected.

Overall payroll risk is classified
as {riskLevel}.

Payroll processing remains operationally
stable but requires management review
of high-risk departments.";
    }
    private List<string> GenerateInsights(
    DashboardSummaryDto dashboard)
    {
        var insights =
            new List<string>();

        insights.Add(
            $"Total anomalies detected: {dashboard.TotalAnomalies}");

        insights.Add(
            $"Pending approvals: {dashboard.TotalPendingApprovals}");

        insights.Add(
            $"Payroll cycles analysed: {dashboard.TotalCycles}");

        var highestRisk =
            dashboard.DepartmentScores
                .OrderByDescending(x => x.RiskScore)
                .FirstOrDefault();

        if (highestRisk != null)
        {
            insights.Add(
                $"{highestRisk.Department} is the highest risk department with score {highestRisk.RiskScore}");
        }

        return insights;
    }
    private List<string> GenerateRecommendations(DashboardSummaryDto dashboard)
    {
        return new List<string>
    {
        "Review salary spike anomalies.",
        "Audit departments with Critical risk levels.",
        "Reduce pending approvals before payroll closure.",
        "Implement manager approval workflow for high-value salary changes.",
        "Monitor recurring payroll anomalies monthly."
    };
    }
}