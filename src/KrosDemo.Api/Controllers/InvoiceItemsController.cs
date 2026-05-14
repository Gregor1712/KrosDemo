using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using KrosDemo.Api.Infrastructure;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Exceptions;
using KrosDemo.Application.Services;

namespace KrosDemo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/invoiceitems")]
public class InvoiceItemsController : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceItemDTO>> UpdateInvoiceItem(
        int id,
        [FromServices] IInvoiceItemService service,
        [FromBody] InvoiceItemUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var rowVersion = ETag.TryParseIfMatch(Request.Headers)
            ?? throw PreconditionRequiredException.MissingIfMatch();

        var updated = await service.UpdateInvoiceItemAsync(id, dto, rowVersion, cancellationToken);
        Response.Headers[HeaderNames.ETag] = ETag.Format(updated.RowVersion);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoiceItem(
        int id,
        [FromServices] IInvoiceItemService service,
        CancellationToken cancellationToken)
    {
        var rowVersion = ETag.TryParseIfMatch(Request.Headers)
            ?? throw PreconditionRequiredException.MissingIfMatch();

        await service.DeleteInvoiceItemAsync(id, rowVersion, cancellationToken);
        return NoContent();
    }
}