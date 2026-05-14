using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using KrosDemo.Api.Infrastructure;

namespace KrosDemo.Tests.Integration;

public class IdempotencyMiddlewareIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public IdempotencyMiddlewareIntegrationTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Post_WithoutIdempotencyKey_NotCached_BothInsert()
    {
        var response1 = await _client.SendAsync(BuildPost(invoiceNumber: "INV-NOKEY-1"));
        var response2 = await _client.SendAsync(BuildPost(invoiceNumber: "INV-NOKEY-2"));

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        Assert.False(response1.Headers.Contains(IdempotencyMiddleware.ReplayHeader));
        Assert.False(response2.Headers.Contains(IdempotencyMiddleware.ReplayHeader));

        var id1 = (await ReadInvoiceAsync(response1)).GetProperty("id").GetInt32();
        var id2 = (await ReadInvoiceAsync(response2)).GetProperty("id").GetInt32();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task Post_WithSameIdempotencyKey_SecondReturnsCachedResponse()
    {
        var key = Guid.NewGuid().ToString();

        var first = BuildPost(invoiceNumber: "INV-IDEM-A");
        first.Headers.Add(IdempotencyMiddleware.HeaderName, key);
        var firstResponse = await _client.SendAsync(first);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.False(firstResponse.Headers.Contains(IdempotencyMiddleware.ReplayHeader));

        var firstBody = await ReadInvoiceAsync(firstResponse);
        var firstId = firstBody.GetProperty("id").GetInt32();
        var firstLocation = firstResponse.Headers.Location?.OriginalString;
        var firstETag = firstResponse.Headers.ETag?.Tag;

        // Replay with the same key — body different on purpose, server must IGNORE it
        // and return the original cached response.
        var second = BuildPost(invoiceNumber: "INV-DIFFERENT-PAYLOAD");
        second.Headers.Add(IdempotencyMiddleware.HeaderName, key);
        var secondResponse = await _client.SendAsync(second);

        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal("true", secondResponse.Headers.GetValues(IdempotencyMiddleware.ReplayHeader).Single());
        Assert.Equal(firstLocation, secondResponse.Headers.Location?.OriginalString);
        Assert.Equal(firstETag, secondResponse.Headers.ETag?.Tag);

        var secondBody = await ReadInvoiceAsync(secondResponse);
        Assert.Equal(firstId, secondBody.GetProperty("id").GetInt32());
        // Replay returns the FIRST request's invoiceNumber, not the second's
        Assert.Equal("INV-IDEM-A", secondBody.GetProperty("invoiceNumber").GetString());
    }

    [Fact]
    public async Task Post_WithDifferentIdempotencyKeys_BothInsert()
    {
        var first = BuildPost(invoiceNumber: "INV-KEY-X");
        first.Headers.Add(IdempotencyMiddleware.HeaderName, Guid.NewGuid().ToString());
        var second = BuildPost(invoiceNumber: "INV-KEY-Y");
        second.Headers.Add(IdempotencyMiddleware.HeaderName, Guid.NewGuid().ToString());

        var firstResponse = await _client.SendAsync(first);
        var secondResponse = await _client.SendAsync(second);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.False(secondResponse.Headers.Contains(IdempotencyMiddleware.ReplayHeader));

        var id1 = (await ReadInvoiceAsync(firstResponse)).GetProperty("id").GetInt32();
        var id2 = (await ReadInvoiceAsync(secondResponse)).GetProperty("id").GetInt32();
        Assert.NotEqual(id1, id2);
    }

    private static HttpRequestMessage BuildPost(string invoiceNumber) =>
        new(HttpMethod.Post, "/api/invoices")
        {
            Content = JsonContent.Create(new
            {
                invoiceNumber,
                customerName = "Idem Customer",
                issueDate = "2026-05-01",
                dueDate = "2026-06-01",
                status = 0,
                currencyCode = "EUR",
                items = Array.Empty<object>()
            })
        };

    private static async Task<JsonElement> ReadInvoiceAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.Clone();
    }
}