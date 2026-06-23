namespace anamoly_detection_api.Models.DTO
{
    public class CycleInformationResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string CycleId { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        // ====================================================
        // PAYROLL ANALYTICS
        // ====================================================

        public int TotalEmployees { get; set; }

        public int EmployeesPaid { get; set; }

        public int PendingApprovals { get; set; }

        public int EmptyCalculations { get; set; }

        public int DuplicateEmployees { get; set; }

        public int NegativeSalaries { get; set; }

        public int HighSalaryAnomalies { get; set; }
        public List<AnomalyFlagDto> Anomalies { get; set; }
       = new();
        public int TotalAnomalies { get; set; }
        //public string AiSummary { get; set; } = string.Empty;
    }
}