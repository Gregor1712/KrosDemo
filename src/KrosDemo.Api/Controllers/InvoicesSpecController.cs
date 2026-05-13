using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Repositories;
using KrosDemo.Application.RequestHelpers;
using KrosDemo.Application.Specifications;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Api.Controllers;

public class InvoicesSpecController(IUnitOfWork unit) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<Pagination<Invoice>>> GetInvoices(
        [FromQuery] InvoiceSpecParams specParams,
        CancellationToken cancellationToken)
    {
        var spec = new InvoiceSpecification(specParams);

        return await CreatePagedResult(
            unit.Repository<Invoice>(),
            spec,
            specParams.PageIndex,
            specParams.PageSize,
            cancellationToken);
    }
}