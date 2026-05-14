using AutoMapper;
using Moq;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Mapping;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Services;

namespace KrosDemo.Tests.Services;

public class InvoiceServiceTests
{
    private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<AutoMapperProfiles>()).CreateMapper();

    [Fact]
    public async Task CreateInvoiceAsync_MapsDtoAndPersists()
    {
        var repo = new Mock<IInvoiceRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice inv, CancellationToken _) =>
            {
                inv.Id = 99;
                inv.RowVersion = [0xAB];
                return inv;
            });

        var sut = new InvoiceService(repo.Object, _mapper);

        var dto = new InvoiceCreateDTO
        {
            InvoiceNumber = "INV-NEW",
            CustomerName = "Cust",
            IssueDate = new DateTime(2026, 01, 01),
            DueDate = new DateTime(2026, 02, 01),
            CurrencyCode = "EUR",
            Items = [new() { Description = "x", Unit = "h", Quantity = 1, UnitPrice = 10, VatRate = 23 }]
        };

        var result = await sut.CreateInvoiceAsync(dto);

        Assert.Equal(99, result.Id);
        Assert.Equal("INV-NEW", result.InvoiceNumber);
        Assert.Single(result.Items);
        repo.Verify(r => r.AddAsync(
            It.Is<Invoice>(i => i.InvoiceNumber == "INV-NEW" && i.Items.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WhenNotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<IInvoiceRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var sut = new InvoiceService(repo.Object, _mapper);

        var dto = new InvoiceUpdateDTO
        {
            InvoiceNumber = "x", CustomerName = "x", CurrencyCode = "EUR",
            IssueDate = DateTime.Today, DueDate = DateTime.Today
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => sut.UpdateInvoiceAsync(404, dto, [], CancellationToken.None));
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WhenFound_MapsDtoOntoExistingAndPreservesItems()
    {
        var existing = new Invoice
        {
            Id = 7,
            InvoiceNumber = "OLD",
            CustomerName = "OldCust",
            CurrencyCode = "EUR",
            IssueDate = new DateTime(2024, 01, 01),
            DueDate = new DateTime(2024, 02, 01),
            Items = [new InvoiceItem { Id = 100, Description = "untouched", Unit = "h", Quantity = 1, UnitPrice = 10, VatRate = 23 }]
        };

        var repo = new Mock<IInvoiceRepository>();
        repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var sut = new InvoiceService(repo.Object, _mapper);

        var dto = new InvoiceUpdateDTO
        {
            InvoiceNumber = "NEW",
            CustomerName = "NewCust",
            CurrencyCode = "USD",
            IssueDate = new DateTime(2026, 05, 14),
            DueDate = new DateTime(2026, 06, 14),
            Status = InvoiceStatus.Paid
        };

        var result = await sut.UpdateInvoiceAsync(7, dto, [0xFF], CancellationToken.None);

        Assert.Equal("NEW", existing.InvoiceNumber);
        Assert.Equal("USD", existing.CurrencyCode);
        Assert.Equal(InvoiceStatus.Paid, existing.Status);
        Assert.Single(existing.Items);
        Assert.Equal("untouched", existing.Items[0].Description);

        Assert.Equal("NEW", result.InvoiceNumber);
        repo.Verify(r => r.UpdateAsync(existing, It.Is<byte[]>(b => b[0] == 0xFF), It.IsAny<CancellationToken>()), Times.Once);
    }
}