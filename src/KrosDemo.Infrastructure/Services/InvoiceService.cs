using AutoMapper;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Interfaces;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IMapper _mapper;

    public InvoiceService(IInvoiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(filter, sort, pagination, cancellationToken);
        var dtos = _mapper.Map<List<InvoiceDTO>>(items);

        return new PagedResponse<List<InvoiceDTO>>(
            dtos,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    public async Task<InvoiceDTO> UpdateInvoiceAsync(
        int id,
        InvoiceUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.CustomerName = dto.CustomerName;
        invoice.CustomerBusinessId = dto.CustomerBusinessId;
        invoice.IssueDate = dto.IssueDate;
        invoice.DueDate = dto.DueDate;
        invoice.Status = dto.Status;
        invoice.CurrencyCode = dto.CurrencyCode;

        await _repository.UpdateAsync(invoice, rowVersion, cancellationToken);
        return _mapper.Map<InvoiceDTO>(invoice);
    }

    public Task DeleteInvoiceAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, rowVersion, cancellationToken);
    }
}