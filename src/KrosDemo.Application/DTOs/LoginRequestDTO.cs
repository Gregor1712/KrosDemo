namespace KrosDemo.Application.DTOs;

public class LoginRequestDTO
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}