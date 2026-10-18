using WclTest.Shared.Services;

namespace WclTest.Api;

public sealed class TokenStorage : ITokenStorage
{
    public string? AccessToken { get; private set; }

    public string? RefreshToken { get; private set; }

    public void Set(string? accessToken, string? refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Reset()
    {
        AccessToken = null;
        RefreshToken = null;
    }
}
