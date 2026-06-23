using Microsoft.AspNetCore.Mvc;
using anamoly_detection_api.Services;

namespace anamoly_detection_api.Controllers
{

    [ApiController]


    [Route("/api/v1/payroll")]

    public class UploadController : ControllerBase
    {

        private readonly IUploadService _csvService;
        private readonly ICycleInformationService _cycleInformationService;
        private readonly IDeleteCycleService _deleteCycleService;

        public UploadController(IUploadService csvService, ICycleInformationService cycleInformationService,IDeleteCycleService deleteCycleService)
        {
            _csvService = csvService;
            _cycleInformationService = cycleInformationService;
            _deleteCycleService = deleteCycleService;
        }

        [HttpPost("upload-test")]
        public IActionResult UploadTest()
        {
            return Ok("Reached");
        }


        #region upload/file
        [HttpPost("upload")]


        // [RequestSizeLimit] → Max file size allowed = 50 MB
        [RequestSizeLimit(52_428_800)]
   
        public async Task<IActionResult> Upload(IFormFile file)
        {
            Console.WriteLine("STEP 1");
            try
            {
                var result = await _csvService.UploadAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
        #endregion

        #region get functions
        [HttpGet("cycles")]
        //list all payroll cycle with status
        public async Task<IActionResult> GetAllCycles()
        {
            var output = await _cycleInformationService.GetAllUploadedFilesAsync();

            if (output == null || output.Count == 0)
            {
                return NotFound("No uploaded files found.");
            }

            return Ok(output);
        }
        [HttpGet("cycles/{cycleId}")]
        // get details of a specific payroll cycle
        public async Task<IActionResult> GetCyclesInformation(string cycleId)
        {
            var output =
                await _cycleInformationService
                .GetCycleInformationAsync(cycleId);

            if (output.Success)
            {
                return Ok(output);
            }

            return BadRequest(output);
        }

    
    #endregion
    [HttpDelete("cycles/{cycleId}")]
        public async Task<IActionResult> DeleteCycle(string cycleId)
        {
            var result = await _deleteCycleService.DeleteCycleAsync(cycleId);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}