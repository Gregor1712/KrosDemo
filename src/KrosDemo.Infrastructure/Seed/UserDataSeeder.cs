using System.Security.Cryptography;
using System.Text;
using KrosDemo.Application.Interfaces;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Seed;

public class UserDataSeeder : IUserDataSeeder
{
    private readonly ApplicationDbContext _context;

    public UserDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedUsers()
    {
        if (!_context.Users.Any())
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("Admin#123"),
                IsAdmin = true
            };
            var user = new User
            {
                Username = "user",
                PasswordHash = HashPassword("User#123"),
                IsAdmin = false
            };

            _context.Users.AddRange(admin, user);
            _context.SaveChanges();
        }

        await Task.CompletedTask;
    }

    public string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}