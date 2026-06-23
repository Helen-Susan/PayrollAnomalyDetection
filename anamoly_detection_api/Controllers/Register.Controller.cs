using anamoly_detection_api.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using anamoly_detection_api.Models;
using anamoly_detection_api.Services;
using anamoly_detection_api.Data;


[ApiController]

public class Register : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public Register (ApplicationDbContext context)
    {
        _context = context;
    }
    #region /register
    [HttpPost("register")]
    public async Task<IActionResult> Register1(Registerdto dto)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
        {
            return BadRequest("User already exists");
        }
        Console.Write(dto.Password);
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

        return Ok(new { message = "User registered successfully" });
    }

    #endregion

}

