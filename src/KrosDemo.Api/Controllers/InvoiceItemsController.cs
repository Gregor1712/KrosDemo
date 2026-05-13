using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using KrosDemo.Api.Infrastructure;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Interfaces;

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
        [FromServices] IMapper mapper,
        [FromBody] InvoiceItemUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateInvoiceItemAsync(id, dto, cancellationToken);
        Response.Headers[HeaderNames.ETag] = ETag.Format(updated.RowVersion);
        return Ok(mapper.Map<InvoiceItemDTO>(updated));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoiceItem(
        int id,
        [FromServices] IInvoiceItemService service,
        CancellationToken cancellationToken)
    {
        var rowVersion = ETag.TryParseIfMatch(Request.Headers);
        if (rowVersion is null)
            return Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "Missing or invalid If-Match header",
                detail: "Provide the resource's current ETag in the If-Match header to perform a conditional delete.");

        await service.DeleteInvoiceItemAsync(id, rowVersion, cancellationToken);
        return NoContent();
    }
}