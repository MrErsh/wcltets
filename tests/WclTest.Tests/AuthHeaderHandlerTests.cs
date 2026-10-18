using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using WclTest.Api;
using WclTest.Api.Http;

namespace WclTest.Tests;

public sealed class AuthHeaderHandlerTests
{
    [Fact]
    public async Task Attaches_Bearer_Token_When_Present()
    {
        var tokens = new TokenStorage();
        tokens.Set("acc_1", "ref_1");
        var sent = await SendThroughHandlerAsync(tokens);

        sent!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        sent.Headers.Authorization!.Parameter.Should().Be("acc_1");
    }

    [Fact]
    public async Task Does_Not_Attach_Header_When_No_Token()
    {
        var tokens = new TokenStorage();
        var sent = await SendThroughHandlerAsync(tokens);

        sent!.Headers.Authorization.Should().BeNull();
    }

    private static async Task<HttpRequestMessage?> SendThroughHandlerAsync(TokenStorage tokens)
    {
        HttpRequestMessage? sent = null;
        var inner = new Mock<HttpMessageHandler>();
        inner.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using var handler = new AuthHeaderHandler(tokens) { InnerHandler = inner.Object };
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api/") };

        await client.GetAsync("auth/me");
        return sent;
    }
}
