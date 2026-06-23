using anamoly_detection_api.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace anamoly_detection_api.Services
{
    public interface IDeleteCycleService
    {
       
        Task<Boolean> DeleteCycleAsync(string cycleId);
    }
}
