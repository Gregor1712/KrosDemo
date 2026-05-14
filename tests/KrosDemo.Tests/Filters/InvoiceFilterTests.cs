using KrosDemo.Application.Filters;
using KrosDemo.Application.Filters.Conditions;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Tests.Filters;

public class InvoiceFilterTests
{
    private static readonly List<Invoice> Sample =
    [
        new() { InvoiceNumber = "INV-001", CustomerName = "Acme",    CurrencyCode = "EUR",
                IssueDate = new DateTime(2024, 01, 15), DueDate = new DateTime(2024, 02, 15) },
        new() { InvoiceNumber = "INV-002", CustomerName = "Globex",  CurrencyCode = "EUR",
                IssueDate = new DateTime(2024, 03, 10), DueDate = new DateTime(2024, 04, 10) },
        new() { InvoiceNumber = "INV-003", CustomerName = "Initech", CurrencyCode = "EUR",
                IssueDate = new DateTime(2024, 06, 01), DueDate = new DateTime(2024, 07, 01) },
    ];

    [Fact]
    public void EmptyFilter_ReturnsAll()
    {
        var filtered = Sample.AsQueryable().ApplyFilter(new InvoiceFilter()).ToList();
        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void IssueDate_GreaterThanOrEqual_FiltersCorrectly()
    {
        var filter = new InvoiceFilter();
        filter.IssueDate.Operator = ConditionType.GreaterThanOrEqual;
        filter.IssueDate.Values.Add(new DateTime(2024, 03, 01));

        var got = Sample.AsQueryable().ApplyFilter(filter).Select(i => i.InvoiceNumber).ToList();
        Assert.Equal(new[] { "INV-002", "INV-003" }, got);
    }

    [Fact]
    public void IssueDate_LessThanOrEqual_FiltersCorrectly()
    {
        var filter = new InvoiceFilter();
        filter.IssueDate.Operator = ConditionType.LessThanOrEqual;
        filter.IssueDate.Values.Add(new DateTime(2024, 03, 10));

        var got = Sample.AsQueryable().ApplyFilter(filter).Select(i => i.InvoiceNumber).ToList();
        Assert.Equal(new[] { "INV-001", "INV-002" }, got);
    }

    [Fact]
    public void IssueDate_Between_FiltersInclusiveLowerExclusiveUpper()
    {
        // DateTimeCondition Between uses >= lower, < upper
        var filter = new InvoiceFilter();
        filter.IssueDate.Operator = ConditionType.Between;
        filter.IssueDate.Values.Add(new DateTime(2024, 02, 01));
        filter.IssueDate.Values.Add(new DateTime(2024, 05, 01));

        var got = Sample.AsQueryable().ApplyFilter(filter).Select(i => i.InvoiceNumber).ToList();
        Assert.Equal(new[] { "INV-002" }, got);
    }

    [Fact]
    public void CustomerName_Contains_FiltersCorrectly()
    {
        var filter = new InvoiceFilter();
        filter.CustomerName!.Operator = ConditionType.Contains;
        filter.CustomerName.Values.Add("tech");

        var got = Sample.AsQueryable().ApplyFilter(filter).Select(i => i.InvoiceNumber).ToList();
        Assert.Equal(new[] { "INV-003" }, got);
    }
}