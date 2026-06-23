using Dapper;
using Microsoft.Data.SqlClient;
using anamoly_detection_api.Models.DTO;
using System.Reflection.Metadata.Ecma335;

namespace anamoly_detection_api.Services
{
    public class RiskScoreService
    : IRiskScoreService
    {
        private readonly string _connectionString;

        public RiskScoreService(
            IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;
        }

        public async Task<List<DepartmentRiskScoreDto>>
            GetDepartmentRiskScoresAsync()
        {
            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            var query = @"
                            SELECT
                                    Department,
                                    Severity,
                                    SUM([Count]) AS Count
                                FROM AnomalyFlags
                                GROUP BY
                                    Department,
                                    Severity";

            var results =
                await connection.QueryAsync<DepartmentRiskScoreDto>(query);
         

            var departmentScores =
    results
    .GroupBy(x => x.Department)
    .Select(g =>
    {
        var totalAnomalies =
            g.Sum(x => x.Count);
        var count = g.Sum(x => x.Count);

        var criticalCount =
            g.Where(x =>
                !string.IsNullOrWhiteSpace(x.Severity) &&
                x.Severity.Trim()
                    .Equals("Critical",
                        StringComparison.OrdinalIgnoreCase))
             .Sum(x => x.Count);

        var highCount =
            g.Where(x =>
                !string.IsNullOrWhiteSpace(x.Severity) &&
                x.Severity.Trim()
                    .Equals("High",
                        StringComparison.OrdinalIgnoreCase))
             .Sum(x => x.Count);

        var riskScore =
            (totalAnomalies * 2)
            + (criticalCount * 10)
            + (highCount * 5);
          

        return new DepartmentRiskScoreDto
        {
            Department = g.Key,

            TotalAnomalies = totalAnomalies,
            Count=count,
         

            CriticalCount = criticalCount,

            HighCount = highCount,
           


            RiskScore = riskScore,

            RiskLevel = GetRiskLevel(riskScore),

            Severity =
                DetermineDepartmentSeverity(g)
        };
    })
    .OrderByDescending(x => x.RiskScore)
    .ToList();
           

          
            return departmentScores;
        }
       

       
        
       
        private string DetermineDepartmentSeverity(
    IGrouping<string,
    DepartmentRiskScoreDto> group)
        {
            if (group.Any(x =>
                x.Severity == "Critical"))
                return "Critical";

            if (group.Any(x =>
                x.Severity == "High"))
                return "High";

            if (group.Any(x =>
                x.Severity == "Medium"))
                return "Medium";

            return "Low";
        }
        private string GetRiskLevel(int score)
        {
            if (score >= 120)
                return "Critical";

            if (score >= 80)
                return "High";

            if (score >= 40)
                return "Medium";

            return "Low";
        }
    }
}
