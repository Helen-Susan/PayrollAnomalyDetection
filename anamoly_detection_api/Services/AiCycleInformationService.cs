using System.Text;
using System.Text.Json;
using anamoly_detection_api.Models.DTO;

namespace anamoly_detection_api.Services
{
    public class AiCycleInformationService : IAiCycleInformationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiCycleInformationService( HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GeneratePayrollSummaryAsync(CycleInformationResponseDto cycleInfo)
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            var prompt = $@"Analyze this payroll cycle .

                        Cycle Id: {cycleInfo.CycleId}
                        Total Employees: {cycleInfo.TotalEmployees}
                        Employees Paid: {cycleInfo.EmployeesPaid}
                        Pending Approvals: {cycleInfo.PendingApprovals}
                        Empty Calculations: {cycleInfo.EmptyCalculations}
                        Duplicate Employees: {cycleInfo.DuplicateEmployees}
                        Negative Salaries: {cycleInfo.NegativeSalaries}
                        High Salary Anomalies: {cycleInfo.HighSalaryAnomalies}

                    Generate a detail table  of the payroll cycle .I will provide how the table  should be structured and you need to follow the structured and generate the table  based on the above nformation. 
                    Table Structure:
                    there should two columns in the table 1.Description and 2. Details
                    2.tell the numerical details of the payroll cycle
                    eg: 3 employess paid ,3 pending approvals ,1 empty calculations,1 duplicate employess, 1 high salary anomaly
                    This cycle involves a total of 7 employees, with some progress made in payments but also several pending items and identified issues.
                    2.    Description        Details
                        1. Cycle Id:            {cycleInfo.CycleId}
                    .   2.Total Employees:     {cycleInfo.TotalEmployees}
                        3.Employees Paid:      {cycleInfo.EmployeesPaid}
                        4.Pending Approvals:   {cycleInfo.PendingApprovals}
                        5.Empty Calculations:  {cycleInfo.EmptyCalculations}
                        6.Duplicate Employees: {cycleInfo.DuplicateEmployees}
                        7.Negative Salaries:   {cycleInfo.NegativeSalaries}
                       8.High Salary Anomalies: {cycleInfo.HighSalaryAnomalies}"


;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            Console.WriteLine($"URL = {url}");
            Console.WriteLine($"API Key Length = {apiKey?.Length}");

            var response = await _httpClient.PostAsync(url, content);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Body:");
            Console.WriteLine(responseText);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Status: {response.StatusCode}\nResponse: {responseText}");
            }

            using var doc = JsonDocument.Parse(responseText);

            return doc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()!;
        }
    }
}