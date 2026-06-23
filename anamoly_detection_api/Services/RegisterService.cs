using Microsoft.EntityFrameworkCore;
using anamoly_detection_api.Data;
using anamoly_detection_api.Models;
using anamoly_detection_api.Services;
using anamoly_detection_api.Models.DTO;
namespace anamoly_detection_api.Services
{
    public class RegisterService : IRegisterSevice
    {
        private readonly ApplicationDbContext _context;
        #region constructor
        public RegisterService(ApplicationDbContext context)
            {
                _context = context;
            }
        #endregion constructor
        #region public methods
        public async Task<LoginResultDto> RegisterAsync(Registerdto dto)
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
    
                if (existingUser != null)
                {
                    return new LoginResultDto
                    {
                        IsSuccess = false,
                        Message = "User already exists"
                    };
                }
    
                // Create new user
                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = PasswordHasher.HashPassword(dto.Password),
                    Position = "null",
                };
    
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
    
                return new LoginResultDto
                {
                    IsSuccess = true,
                    Message = "User registered successfully"
                };
        }
    }
        #endregion public methods
}
