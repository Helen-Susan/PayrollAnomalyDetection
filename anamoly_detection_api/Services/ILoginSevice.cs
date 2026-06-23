// Services/ILoginService.cs

using anamoly_detection_api.Models;

namespace anamoly_detection_api.Services
{
    /// <summary>
    /// Contract for Login Service.
    /// Any class implementing this MUST provide these method implementations.
    
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Requirement: Validate user credentials against the database.
        /// - Must check if the user exists by email
        /// - Must verify the hashed password
        /// - Must return user info on success or an error message on failure
        /// </summary>
        /// <param name="dto">Login data containing Email and Password</param>
        /// <returns>
        ///     Success → LoginResultDto with user details
        ///     Failure → LoginResultDto with error message and success = false
        /// </returns>
        Task<LoginResultDto> LoginAsync(LoginDto dto);
    }
}