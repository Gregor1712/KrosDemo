using AutoMapper;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Repositories;
using KrosDemo.Application.Services;

namespace KrosDemo.Infrastructure.Services;

public class InvoiceItemService : IInvoiceItemService
{
    private readonly IInvoiceItemRepository _repository;
    private readonly IMapper _mapper;

    public InvoiceItemService(IInvoiceItemRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<InvoiceItemDTO> UpdateInvoiceItemAsync(
        int id,
        InvoiceItemUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"InvoiceItem {id} not found.");

        item.Description = dto.Description;
        item.Unit = dto.Unit;
        item.Quantity = dto.Quantity;
        item.UnitPrice = dto.UnitPrice;
        item.VatRate = dto.VatRate;

        await _repository.UpdateAsync(item, rowVersion, cancellationToken);
        return _mapper.Map<InvoiceItemDTO>(item);
    }

    public Task DeleteInvoiceItemAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, rowVersion, cancellationToken);
    }
}