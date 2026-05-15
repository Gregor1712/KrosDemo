using AutoMapper;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Repositories;
//using KrosDemo.Application.RequestHelpers;
using KrosDemo.Application.Services;

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

    public async Task<InvoiceDTO> GetInvoiceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        return _mapper.Map<InvoiceDTO>(invoice);
    }

    public async Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(filter, sort, pagination, cancellationToken);
        var dtos = _mapper.Map<List<InvoiceDTO>>(result);
        return new(dtos, pagination, result.TotalRecords);
    }

    public async Task<InvoiceDTO> CreateInvoiceAsync(
        InvoiceCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var invoice = _mapper.Map<Domain.Entities.Invoice>(dto);
        await _repository.AddAsync(invoice, cancellationToken);
        return _mapper.Map<InvoiceDTO>(invoice);
    }

    public async Task<InvoiceDTO> UpdateInvoiceAsync(
        int id,
        InvoiceUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        _mapper.Map(dto, invoice);
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