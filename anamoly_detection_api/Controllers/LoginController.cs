// Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using anamoly_detection_api.Models;
using anamoly_detection_api.Services;

namespace anamoly_detection_api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;
        
        /// <summary>
        /// Requirement: Controller depends on ILoginService interface — NOT LoginService directly.
        /// This follows Dependency Inversion Principle.
        /// .NET DI container automatically injects LoginService at runtime.
        /// ApplicationDbContext is NO LONGER injected here — LoginService handles it.
        /// </summary>
        public AuthController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        /// <summary>
        /// Requirement: Health check endpoint — confirms API is running.
        /// No business logic here.
        /// </summary>


        /// <summary>
        /// Requirement: Login endpoint — handles ONLY HTTP concerns.
        /// 1. Receives LoginDto from request body
        /// 2. Delegates ALL logic to ILoginService
        /// 3. Maps the result to appropriate HTTP response (200 or 401)
        /// Controller does NOT touch DB, hash passwords, or contain any business rules.
        /// </summary>
        #region Login Endpoint

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Delegate all login logic to the service
           var  result= await _loginService.LoginAsync(dto);

            // Map service result → HTTP response (controller's only job)
            if (!result.IsSuccess)
            {
                return Unauthorized(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                user = result.User
            });
        }
        #endregion

    }
}