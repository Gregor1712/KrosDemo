using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Set<T>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<T>().ToListAsync(cancellationToken);
    }

    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        context.Set<T>().Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Set<T>().AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<T?> GetEntityWithSpec(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public async Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(context.Set<T>().AsQueryable(), spec);
    }

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
    {
        return SpecificationEvaluator<T>.GetQuery<T, TResult>(context.Set<T>().AsQueryable(), spec);
    }

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        var query = context.Set<T>().AsQueryable();
        query = spec.ApplyCriteria(query);
        return await query.CountAsync(cancellationToken);
    }
}