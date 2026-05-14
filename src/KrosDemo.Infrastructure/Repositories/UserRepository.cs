using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}