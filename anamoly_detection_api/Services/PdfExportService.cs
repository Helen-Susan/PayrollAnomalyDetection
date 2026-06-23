using anamoly_detection_api.Models.DTO;
using anamoly_detection_api.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PdfExportService : IPdfExportService
{
    private readonly IExecutiveReportAgentService _executiveReportService;
    private readonly IReportService _reportService;
    private readonly IRiskScoreService _riskScoreService;

    public PdfExportService(
        IExecutiveReportAgentService executiveReportService,
        IReportService reportService,
        IRiskScoreService riskScoreService)
    {
        _executiveReportService = executiveReportService;
        _reportService = reportService;
        _riskScoreService = riskScoreService;
    }

    public async Task<byte[]> GenerateAnalyticsPdfAsync()
    {
        var executiveSummary =
            await _executiveReportService
                .GenerateExecutiveReportAsync();

        var summary =
            await _reportService
                .GetSummaryAsync();

        var risks =
            await _riskScoreService
                .GetDepartmentRiskScoresAsync();

        QuestPDF.Settings.License =
            LicenseType.Community;

        var document =
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(25);

                    page.DefaultTextStyle(x =>
                        x.FontSize(11));

                    /* HEADER */

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Payroll Intelligence Report")
                                .FontSize(28)
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}"
                                )
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    /* CONTENT */

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            /* EXECUTIVE SUMMARY */

                            column.Item()
                                .Text("Executive Summary")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Text(executiveSummary);

                            /* DASHBOARD OVERVIEW */

                            column.Item()
                                .PaddingTop(10);

                            column.Item()
                                .Text("Dashboard Overview")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell().Text("Payroll Cycles");
                                    table.Cell().Text(summary.TotalCycles.ToString());

                                    table.Cell().Text("Total Employees");
                                    table.Cell().Text(summary.TotalEmployees.ToString());

                                    table.Cell().Text("Employees Paid");
                                    table.Cell().Text(summary.TotalEmployeesPaid.ToString());

                                    table.Cell().Text("Pending Approvals");
                                    table.Cell().Text(summary.TotalPendingApprovals.ToString());

                                    table.Cell().Text("Total Anomalies");
                                    table.Cell().Text(summary.TotalAnomalies.ToString());
                                });

                            /* CYCLE TREND */

                            column.Item()
                                .PaddingTop(10);

                            column.Item()
                                .Text("Payroll Cycle Analysis")
                                .FontSize(18)
                                .Bold();

                            foreach (var cycle in summary.CycleTrend)
                            {
                                column.Item()
                                    .Text(
                                        $"{cycle.CycleId} : {cycle.AnomalyCount} anomalies"
                                    );
                            }

                            /* RISK ANALYSIS */

                            column.Item()
                                .PaddingTop(10);

                            column.Item()
                                .Text("Department Risk Analysis")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Department").Bold();
                                        header.Cell().Text("Score").Bold();
                                        header.Cell().Text("Level").Bold();
                                        header.Cell().Text("Anomalies").Bold();
                                    });

                                    foreach (var risk in risks)
                                    {
                                        table.Cell().Text(risk.Department);

                                        table.Cell()
                                            .Text(
                                                risk.RiskScore.ToString());

                                        table.Cell()
                                            .Text(
                                                risk.RiskLevel);

                                        table.Cell()
                                            .Text(
                                                risk.TotalAnomalies.ToString());
                                    }
                                });

                            /* TOP RISK DEPARTMENT */

                            var topRisk =
                                risks
                                .OrderByDescending(x => x.RiskScore)
                                .FirstOrDefault();

                            if (topRisk != null)
                            {
                                column.Item()
                                    .PaddingTop(10);

                                column.Item()
                                    .Text("Highest Risk Department")
                                    .FontSize(18)
                                    .Bold();

                                column.Item()
                                    .Text(
                                        $"{topRisk.Department} has the highest risk score of {topRisk.RiskScore} and is classified as {topRisk.RiskLevel}."
                                    );
                            }
                        });

                    /* FOOTER */

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Payroll Intelligence Platform | Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

        return document.GeneratePdf();
    }
}