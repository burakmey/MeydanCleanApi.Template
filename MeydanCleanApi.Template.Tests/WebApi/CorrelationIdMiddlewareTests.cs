using MeydanCleanApi.Template.Infrastructure.Services.Tracing;
using MeydanCleanApi.Template.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MeydanCleanApi.Template.Tests.WebApi;

/// <summary>
/// Covers which inbound correlation ids are trusted. The value reaches the log file, so anything
/// accepted here is something a caller can put in the log.
/// </summary>
public sealed class CorrelationIdMiddlewareTests
{
    private static async Task<string> ResolveAsync(string? inboundHeader)
    {
        var context = new DefaultHttpContext();

        if (inboundHeader is not null)
        {
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = inboundHeader;
        }

        var correlationIdContext = new CorrelationIdContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, correlationIdContext);

        Assert.Equal(correlationIdContext.CorrelationId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());

        return correlationIdContext.CorrelationId;
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("ci-check-1")]
    [InlineData("trace_42")]
    [InlineData("0f8b2c1d4e5a6b7c8d9e0f1a2b3c4d5e")]
    public async Task APlainIdentifier_IsKept(string inbound)
    {
        Assert.Equal(inbound, await ResolveAsync(inbound));
    }

    [Theory]
    [InlineData("has spaces")]
    [InlineData("semi;colon")]
    [InlineData("quote\"mark")]
    [InlineData("slash/es")]
    [InlineData("unicode-ü")]
    public async Task AnythingBeyondLettersDigitsDashAndUnderscore_IsReplaced(string inbound)
    {
        Assert.NotEqual(inbound, await ResolveAsync(inbound));
    }

    [Theory]
    [InlineData("first\r\nfabricated log line")]
    [InlineData("first\nfabricated log line")]
    [InlineData("tab\there")]
    public async Task AValueThatCouldStartANewLogLine_IsReplaced(string inbound)
    {
        // The regression this guards: the middleware used to check only the length, so a carriage
        // return ended the log line and let the caller compose an entry of its own after it.
        var resolved = await ResolveAsync(inbound);

        Assert.NotEqual(inbound, resolved);
        Assert.False(resolved.Contains('\r'));
        Assert.False(resolved.Contains('\n'));
    }

    [Fact]
    public async Task AnOverlongId_IsReplaced()
    {
        var inbound = new string('a', 129);

        Assert.NotEqual(inbound, await ResolveAsync(inbound));
    }

    [Fact]
    public async Task AnIdAtTheLengthLimit_IsKept()
    {
        var inbound = new string('a', 128);

        Assert.Equal(inbound, await ResolveAsync(inbound));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AMissingOrEmptyHeader_GetsAGeneratedId(string? inbound)
    {
        var resolved = await ResolveAsync(inbound);

        Assert.Equal(32, resolved.Length);
        Assert.True(resolved.All(char.IsAsciiLetterOrDigit));
    }
}
