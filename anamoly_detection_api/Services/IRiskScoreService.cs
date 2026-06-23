namespace anamoly_detection_api.Services
{
    public interface IRiskScoreService
    {
        Task<List<DepartmentRiskScoreDto>> GetDepartmentRiskScoresAsync();
    }
}
