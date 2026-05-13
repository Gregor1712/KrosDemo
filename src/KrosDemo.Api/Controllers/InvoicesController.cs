using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<List<InvoiceDTO>>>> GetInvoices(
        [FromServices] IServerService server,
        [FromQuery] InvoiceFilter filter,
        [FromQuery] SortFilter sort,
        [FromQuery] PaginationFilter pagination)
    {
        var data = await server.GetInvoices(filter, sort, pagination);
        return Ok(data);
    }
}