using Microsoft.Net.Http.Headers;

namespace KrosDemo.Api.Infrastructure;

public static class ETag
{
    public static string Format(byte[] rowVersion) => $"\"{Convert.ToBase64String(rowVersion)}\"";

    public static byte[]? TryParseIfMatch(IHeaderDictionary headers)
    {
        var raw = headers[HeaderNames.IfMatch].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (value.StartsWith("W/", StringComparison.Ordinal))
            value = value.Substring(2);
        value = value.Trim('"');

        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}