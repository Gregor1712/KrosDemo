using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Services;

namespace KrosDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromServices] IAuthService authService,
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.RegisterAsync(request.Username, request.Password, cancellationToken);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromServices] IAuthService authService,
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.LoginAsync(request.Username, request.Password, cancellationToken);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Username, string Password);
}