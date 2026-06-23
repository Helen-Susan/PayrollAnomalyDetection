using anamoly_detection_api.Models.DTO;
using anamoly_detection_api.Services;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

public class ExecutiveReportAgentService: IExecutiveReportAgentService
{
    private readonly IConfiguration _configuration;
    private readonly IReportService _reportService;

    public ExecutiveReportAgentService(IConfiguration configuration,
        IReportService reportService)
    {
        _configuration = configuration;
        _reportService = reportService;
    }


    public async Task<ExecutiveReportResponseDto>
     GenerateExecutiveReportAsync()
    {
        var dashboard =
            await _reportService.GetSummaryAsync();

        var json =
            JsonSerializer.Serialize(
                dashboard,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        var endpoint =
            _configuration["AzureOpenAI:Endpoint"];

        var apiKey =
            _configuration["AzureOpenAI:ApiKey"];

        var deploymentName =
            _configuration["AzureOpenAI:DeploymentName"];

        var client =
            new AzureOpenAIClient(
                new Uri(endpoint!),
                new AzureKeyCredential(apiKey!));

        var chatClient =
            client.GetChatClient(
                deploymentName!);

        Console.WriteLine($"Endpoint: {endpoint}");
        Console.WriteLine($"Deployment: {deploymentName}");
        Console.WriteLine($"ApiKey Exists: {!string.IsNullOrEmpty(apiKey)}");
        var schemma = """
            
            {
            "executiveSummary": "string",
             "payrollHealthScore": 0,
              "riskLevel": "Low",
               "keyInsights": [
                 "string"
                             ],
               "recommendations": [
                            "string"
                  ],
                "conclusion": "string"
                 } 
            
            """;
        var prompt = $"""
You are an Executive Payroll Intelligence Analyst.

Analyze the payroll dashboard data provided below.

Payroll Data:

{json}

IMPORTANT INSTRUCTIONS:

- Use ONLY the supplied payroll data.
- Do NOT invent numbers.
- Return ONLY valid JSON.
- Do NOT return markdown.
- Do NOT wrap the response in ```json.
- Every field must be populated.

- executiveSummary should be concise, professional and suitable for executives.

- payrollHealthScore must be a value between 0 and 100.

- riskLevel must be one of:
  Low
  Medium
  High
  Critical

- keyInsights must contain between 3 and 5 business insights derived from the data.

- recommendations must contain between 3 and 5 actionable recommendations.

Recommendations should focus on:
  * payroll governance
  * anomaly reduction
  * payroll compliance
  * approval workflow improvements
  * department risk mitigation
  * audit readiness

Example recommendations:

[
  "Review payroll cycles with unusually high anomaly counts.",
  "Perform focused audits on high-risk departments.",
  "Reduce pending approvals before payroll closure.",
  "Implement approval workflows for salary adjustments.",
  "Monitor recurring anomalies through monthly reviews."
]

Return JSON matching EXACTLY this schema:

{schemma}

Return ONLY the JSON object.
""";

        var response =
            await chatClient.CompleteChatAsync(
            [
                new UserChatMessage(prompt)
            ]);

        var aiResponse =
            response
                .Value
                .Content[0]
                .Text;

        Console.WriteLine("========== AI RESPONSE ==========");
        Console.WriteLine(aiResponse);
        Console.WriteLine("=================================");

        try
        {
            var report =
                JsonSerializer.Deserialize<
                    ExecutiveReportResponseDto>(
                        aiResponse,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

            if (report == null)
            {
                throw new Exception(
                    "AI returned null report");
            }

            report.DashboardSummary =
                dashboard;

            return report;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"JSON Parse Error: {ex.Message}");

            return new ExecutiveReportResponseDto
            {
                ExecutiveSummary =
                    "Unable to generate executive report.",

                PayrollHealthScore = 0,

                RiskLevel = "Unknown",

                KeyInsights =
                [
                    "AI report generation failed."
                ],

                Recommendations =
                [
                    "Review AI response format."
                ],

                Conclusion =
                    "Executive report could not be generated.",

                DashboardSummary = dashboard
            };
        }
    }
}