namespace KrosDemo.Application.Interfaces;

public interface IUserDataSeeder
{
    Task SeedUsers();
    string HashPassword(string password);
}