using Microsoft.Extensions.Diagnostics.HealthChecks;

public class DepartmentRiskScoreDto
{
    public string Department { get; set; } = string.Empty;

    public int Count { get; set; }

    public int TotalAnomalies { get; set; }

    public int CriticalCount { get; set; }

    public int HighCount { get; set; }

    public int RiskScore { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;
}


