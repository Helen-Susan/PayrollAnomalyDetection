using anamoly_detection_api.Models.DTO;
using anamoly_detection_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace anamoly_detection_api.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IRiskScoreService _RiskScoreService;
       private readonly IPdfExportService _pdfExportService;
        private readonly IExecutiveReportAgentService _executiveReportAgentService;
        public ReportController(IReportService reportService, IRiskScoreService riskScoreService,IExecutiveReportAgentService executiveReportAgentService,IPdfExportService pdfExportService)
        {
            _reportService = reportService;
            _RiskScoreService = riskScoreService;
            _pdfExportService = pdfExportService;
            _executiveReportAgentService = executiveReportAgentService;
        }

        [HttpGet("risk-score")]
        public async Task<IActionResult> RisKScore()
        {
            var risks =
                 await _RiskScoreService
                     .GetDepartmentRiskScoresAsync();


            return Ok(risks);
        }
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>>
       GetDashboard()
        {
            var dashboard =
                await _reportService
                    .GetSummaryAsync();

            return Ok(dashboard);
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes =
                await _pdfExportService
                    .GenerateAnalyticsPdfAsync();

            return File(
                pdfBytes,
                "application/pdf",
                "PayrollAnalyticsReport.pdf");

        }
        [HttpGet("executive")]
        public async Task<IActionResult>
    GenerateExecutiveReport()
        {
            var report =
                await _executiveReportAgentService
                    .GenerateExecutiveReportAsync();

            return Ok(report);
        }
    }
}


