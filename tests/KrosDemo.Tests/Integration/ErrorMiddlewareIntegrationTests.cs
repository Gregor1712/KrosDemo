using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using KrosDemo.Api.Infrastructure;

namespace KrosDemo.Tests.Integration;

public class ErrorMiddlewareIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;

    public ErrorMiddlewareIntegrationTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_Invoices_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/invoices?PageNumber=1&PageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutIfMatchHeader_Returns428WithProblemJson()
    {
        var response = await _client.DeleteAsync($"/api/invoices/{TestWebAppFactory.SeededInvoiceId}");

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadProblemAsync(response);
        Assert.Equal(428, problem.GetProperty("status").GetInt32());
        Assert.Equal("Precondition required", problem.GetProperty("title").GetString());
        Assert.Contains("If-Match", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Put_NonExistentInvoice_Returns404WithProblemJson()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/invoices/999999")
        {
            Content = JsonContent.Create(new
            {
                invoiceNumber = "X",
                customerName = "X",
                issueDate = "2026-01-01",
                dueDate = "2026-02-01",
                status = 0,
                currencyCode = "EUR"
            })
        };
        request.Headers.TryAddWithoutValidation("If-Match", ETag.Format(_factory.SeededRowVersion));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadProblemAsync(response);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("Resource not found", problem.GetProperty("title").GetString());
        Assert.Contains("999999", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Put_WithValidIfMatch_Returns200AndBumpsETag()
    {
        var originalETag = ETag.Format(_factory.SeededRowVersion);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/invoices/{TestWebAppFactory.SeededInvoiceId}")
        {
            Content = JsonContent.Create(new
            {
                invoiceNumber = "INV-TEST",
                customerName = "Updated Customer",
                issueDate = "2026-01-15",
                dueDate = "2026-02-15",
                status = 2,
                currencyCode = "USD"
            })
        };
        request.Headers.TryAddWithoutValidation("If-Match", originalETag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var newETag = response.Headers.ETag?.Tag;
        Assert.NotNull(newETag);
        Assert.NotEqual(originalETag, newETag);

        var body = await ReadProblemAsync(response);
        Assert.Equal("Updated Customer", body.GetProperty("customerName").GetString());
        Assert.Equal("USD", body.GetProperty("currencyCode").GetString());
        Assert.Equal(2, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Put_WithStaleIfMatch_Returns409WithExtensions()
    {
        var staleEtag = ETag.Format([0, 0, 0, 0, 0, 0, 0, 0]);
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/invoices/{TestWebAppFactory.SeededInvoiceId}")
        {
            Content = JsonContent.Create(new
            {
                invoiceNumber = "INV-TEST",
                customerName = "Updated",
                issueDate = "2026-01-15",
                dueDate = "2026-02-15",
                status = 0,
                currencyCode = "EUR"
            })
        };
        request.Headers.TryAddWithoutValidation("If-Match", staleEtag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await ReadProblemAsync(response);
        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.Equal("Concurrency conflict", problem.GetProperty("title").GetString());
        Assert.Equal("Invoice", problem.GetProperty("entityName").GetString());
        Assert.Equal(TestWebAppFactory.SeededInvoiceId, problem.GetProperty("entityId").GetInt32());
        Assert.True(problem.TryGetProperty("currentDatabaseValues", out _));
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.Clone();
    }
}