using AutoMapper;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Application.Services;

public class ServerService : IServerService
{
    private readonly IInvoiceService _invoiceService;
    private readonly IMapper _mapper;

    public ServerService(IInvoiceService invoiceService, IMapper mapper)
    {
        _invoiceService = invoiceService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination)
    {
        var (items, totalCount) = await _invoiceService.GetInvoices(filter, sort, pagination);
        var dtos = _mapper.Map<List<InvoiceDTO>>(items);

        return new PagedResponse<List<InvoiceDTO>>(
            dtos,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }
}