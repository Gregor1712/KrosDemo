using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.RequestHelpers;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected async Task<ActionResult> CreatePagedResult<T>(
        IGenericRepository<T> repository,
        ISpecification<T> specification,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var items = await repository.ListAsync(specification, cancellationToken);
        var totalItems = await repository.CountAsync(specification, cancellationToken);
        var pagination = new Pagination<T>(pageIndex, pageSize, totalItems, items);
        return Ok(pagination);
    }

    protected async Task<ActionResult> CreatePagedResult<T, TDto>(
        IGenericRepository<T> repo,
        ISpecification<T> spec,
        int pageIndex,
        int pageSize,
        Func<T, TDto> toDto,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, IDtoConvertible
        where TDto : class
    {
        var items = await repo.ListAsync(spec, cancellationToken);
        var count = await repo.CountAsync(spec, cancellationToken);

        var dtoItems = items.Select(toDto).ToList();

        var pagination = new Pagination<TDto>(pageIndex, pageSize, count, dtoItems);

        return Ok(pagination);
    }
}