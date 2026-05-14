using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using KrosDemo.Api.Infrastructure;

namespace KrosDemo.Tests.Infrastructure;

public class ETagTests
{
    [Fact]
    public void Format_ProducesQuotedBase64()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        Assert.Equal("\"AQID/w==\"", ETag.Format(bytes));
    }

    [Fact]
    public void TryParseIfMatch_QuotedBase64_ReturnsBytes()
    {
        var headers = new HeaderDictionary { [HeaderNames.IfMatch] = "\"AQID/w==\"" };
        var bytes = ETag.TryParseIfMatch(headers);
        Assert.NotNull(bytes);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0xFF }, bytes);
    }

    [Fact]
    public void TryParseIfMatch_WeakValidator_ReturnsBytes()
    {
        var headers = new HeaderDictionary { [HeaderNames.IfMatch] = "W/\"AQID/w==\"" };
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0xFF }, ETag.TryParseIfMatch(headers));
    }

    [Fact]
    public void TryParseIfMatch_Missing_ReturnsNull()
    {
        Assert.Null(ETag.TryParseIfMatch(new HeaderDictionary()));
    }

    [Theory]
    [InlineData("not-base64!")]
    [InlineData("\"!!!nope!!!\"")]
    public void TryParseIfMatch_Malformed_ReturnsNull(string raw)
    {
        var headers = new HeaderDictionary { [HeaderNames.IfMatch] = raw };
        Assert.Null(ETag.TryParseIfMatch(headers));
    }

    [Fact]
    public void Format_And_Parse_RoundTrip()
    {
        var original = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var headers = new HeaderDictionary { [HeaderNames.IfMatch] = ETag.Format(original) };
        Assert.Equal(original, ETag.TryParseIfMatch(headers));
    }
}