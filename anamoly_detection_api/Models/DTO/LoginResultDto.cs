// Models/LoginResultDto.cs

namespace anamoly_detection_api.Models;

using anamoly_detection_api.Models;
/// <summary>
/// Requirement: Standardized response model returned from LoginService to Controller.
/// Controller uses this to decide what HTTP response to send back.
/// Keeps the service decoupled from HTTP/IActionResult concerns.
/// </summary>
public class LoginResultDto
    {
        /// <summary>
        /// Requirement: Tells the controller whether login succeeded or failed.
        /// true  → return 200 OK
        /// false → return 401 Unauthorized
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Requirement: Human-readable message to return to the frontend.
        /// Example: "Login Successful" or "Invalid password"
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Requirement: Holds authenticated user's basic info.
        /// Only populated when IsSuccess = true.
        /// null when login fails (no user data to expose on failure).
        /// </summary>
        
    

    /// <summary>
    /// Requirement: Slim user object to return to frontend.
    /// Only exposes non-sensitive fields — never return PasswordHash.
    /// </summary>
    public LoginDto? User { get; set; }

}