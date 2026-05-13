using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<bool> Complete(CancellationToken cancellationToken = default);
}