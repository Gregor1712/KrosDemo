using System.Collections.Concurrent;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public async Task<bool> Complete(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public void Dispose()
    {
        context.Dispose();
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;

        return (IGenericRepository<TEntity>)_repositories.GetOrAdd(type, t =>
        {
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(TEntity));
            return Activator.CreateInstance(repositoryType, context)
                   ?? throw new InvalidOperationException($"Could not create repository instance for {t}.");
        });
    }
}