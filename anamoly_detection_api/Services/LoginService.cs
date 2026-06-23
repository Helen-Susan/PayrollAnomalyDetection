// Services/LoginService.cs

using Microsoft.EntityFrameworkCore;
using anamoly_detection_api.Data;
using anamoly_detection_api.Models;

namespace anamoly_detection_api.Services
{
   
    public class LoginService : ILoginService
    {
        private readonly ApplicationDbContext _context;
        #region constructor
        public LoginService(ApplicationDbContext context)
        {
            _context = context;
        }
        #endregion constructor

        #region public methods
        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            //check if user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                
                return new LoginResultDto
                {
                    IsSuccess = false,
                    Message = "User does not exist"
                };
            }

         
            var hashedPassword = PasswordHasher.HashPassword(dto.Password);

       
            if (user.PasswordHash != hashedPassword)
            {
                return new LoginResultDto
                {
                    IsSuccess = false,
                    Message = "Invalid password"
                };
            }
            // create new user
            return new LoginResultDto
            {
                IsSuccess = true,
                Message = "Login Successful",
                User = new LoginDto
                {
                  
                    
                    Email = user.Email,
                    DisplayName=user.Name
                }
            };
        }
    }
        #endregion
}