using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}