using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace KrosDemo.Api.Infrastructure;

// Stripe-style POST idempotency. Client opts in by sending the Idempotency-Key
// header; the first request runs normally and the response is cached for 24h.
// Subsequent requests with the same key + path return the cached response
// verbatim (status, body, key headers) and never reach the controller.
public class IdempotencyMiddleware
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayHeader = "X-Idempotent-Replay";
    private static readonly TimeSpan CacheWindow = TimeSpan.FromHours(24);

    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !context.Request.Headers.TryGetValue(HeaderName, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            await _next(context);
            return;
        }

        var key = keyValues.First()!;
        var cacheKey = $"idem:{context.Request.Method}:{context.Request.Path}:{key}";

        var cachedBytes = await _cache.GetAsync(cacheKey, context.RequestAborted);
        if (cachedBytes is not null)
        {
            var cached = JsonSerializer.Deserialize<CachedResponse>(cachedBytes)!;
            await ReplayAsync(context, cached);
            _logger.LogInformation("Idempotency replay for key {Key} on {Path}", key, context.Request.Path);
            return;
        }

        // Capture downstream response into a buffer so we can both forward
        // it to the client and cache the bytes. This works for buffered
        // JSON responses; streaming responses would need a different approach.
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            buffer.Position = 0;
            var bodyBytes = buffer.ToArray();

            context.Response.Body = originalBody;
            if (bodyBytes.Length > 0)
                await originalBody.WriteAsync(bodyBytes, context.RequestAborted);

            if (ShouldCache(context.Response.StatusCode))
            {
                var entry = new CachedResponse(
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    context.Response.Headers.Location.FirstOrDefault(),
                    context.Response.Headers.ETag.FirstOrDefault(),
                    bodyBytes);

                await _cache.SetAsync(
                    cacheKey,
                    JsonSerializer.SerializeToUtf8Bytes(entry),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheWindow },
                    context.RequestAborted);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool ShouldCache(int status) => status is >= 200 and < 500 and not 500;

    private static async Task ReplayAsync(HttpContext context, CachedResponse cached)
    {
        context.Response.StatusCode = cached.StatusCode;
        if (cached.ContentType is not null) context.Response.ContentType = cached.ContentType;
        if (cached.Location is not null) context.Response.Headers.Location = cached.Location;
        if (cached.ETag is not null) context.Response.Headers.ETag = cached.ETag;
        context.Response.Headers[ReplayHeader] = "true";

        if (cached.Body.Length > 0)
            await context.Response.Body.WriteAsync(cached.Body, context.RequestAborted);
    }

    private record CachedResponse(
        int StatusCode,
        string? ContentType,
        string? Location,
        string? ETag,
        byte[] Body);
}