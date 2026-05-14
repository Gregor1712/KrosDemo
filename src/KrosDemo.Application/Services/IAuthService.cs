namespace KrosDemo.Application.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<string> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}