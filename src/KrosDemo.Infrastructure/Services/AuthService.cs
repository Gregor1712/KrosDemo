using System.Security.Cryptography;
using System.Text;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.Repositories;
using KrosDemo.Application.Services;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<string> RegisterAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsAsync(username, cancellationToken))
            throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            IsAdmin = false
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return _jwtService.GenerateToken(user);
    }

    public async Task<string> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user == null || HashPassword(password) != user.PasswordHash)
            throw new UnauthorizedAccessException("Invalid credentials");

        return _jwtService.GenerateToken(user);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}