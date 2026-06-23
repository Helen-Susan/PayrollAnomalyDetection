using anamoly_detection_api.Models;
using anamoly_detection_api.Models.DTO;
namespace anamoly_detection_api.Services
{
    public interface IRegisterSevice
    {
        Task<LoginResultDto> RegisterAsync(Registerdto dto);
        
    }
}
