using anamoly_detection_api.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace anamoly_detection_api.Services
{
    public interface ICycleInformationService
    {
        Task<CycleInformationResponseDto> GetCycleInformationAsync(string cycleId);
        Task<List<AllCycleHistory>> GetAllUploadedFilesAsync();
     
    }
}
