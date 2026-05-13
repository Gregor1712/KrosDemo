using System.Text;
using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Interfaces;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = Convert.ToBase64String(sha.ComputeHash(bytes));
        return hash == storedHash;
    }

    public record LoginRequest(string Username, string Password);
}