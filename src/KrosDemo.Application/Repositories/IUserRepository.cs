using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}