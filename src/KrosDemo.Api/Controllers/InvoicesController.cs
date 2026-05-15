using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using KrosDemo.Api.Infrastructure;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Exceptions;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Services;

namespace KrosDemo.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    //[Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDTO>> GetInvoiceById(
        int id,
        [FromServices] IInvoiceService service,
        CancellationToken cancellationToken)
    {
        var invoice = await service.GetInvoiceByIdAsync(id, cancellationToken);
        Response.Headers[HeaderNames.ETag] = ETag.Format(invoice.RowVersion);
        return Ok(invoice);
    }

    //[Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<List<InvoiceDTO>>>> GetInvoices(
        [FromServices] IInvoiceService service,
        [FromQuery] InvoiceFilter filter,
        [FromQuery] SortFilter sort,
        [FromQuery] PaginationFilter pagination,
        CancellationToken cancellationToken)
    {
        var result = await service.GetInvoices(filter, sort, pagination, cancellationToken);
        return Ok(result);
    }

    //[Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<InvoiceDTO>> CreateInvoice(
        [FromServices] IInvoiceService service,
        [FromBody] InvoiceCreateDTO dto,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateInvoiceAsync(dto, cancellationToken);
        Response.Headers[HeaderNames.ETag] = ETag.Format(created.RowVersion);
        return Created($"api/invoices/{created.Id}", created);
    }

    //[Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceDTO>> UpdateInvoice(
        int id,
        [FromServices] IInvoiceService service,
        [FromBody] InvoiceUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var rowVersion = ETag.TryParseIfMatch(Request.Headers)
            ?? throw PreconditionRequiredException.MissingIfMatch();

        var updated = await service.UpdateInvoiceAsync(id, dto, rowVersion, cancellationToken);
        Response.Headers[HeaderNames.ETag] = ETag.Format(updated.RowVersion);
        return Ok(updated);
    }

    //[Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoice(
        int id,
        [FromServices] IInvoiceService service,
        CancellationToken cancellationToken)
    {
        var rowVersion = ETag.TryParseIfMatch(Request.Headers)
            ?? throw PreconditionRequiredException.MissingIfMatch();

        await service.DeleteInvoiceAsync(id, rowVersion, cancellationToken);
        return NoContent();
    }
}