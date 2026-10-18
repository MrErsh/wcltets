using System.Net.Http.Headers;
using WclTest.Shared.Services;

namespace WclTest.Api.Http;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokens;

    public AuthHeaderHandler(ITokenStorage tokens) => _tokens = tokens;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokens.AccessToken is { Length: > 0 } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
