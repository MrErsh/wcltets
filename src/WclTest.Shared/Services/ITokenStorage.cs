namespace WclTest.Shared.Services;

public interface ITokenStorage
{
    string? AccessToken { get; }

    string? RefreshToken { get; }

    void Set(string? accessToken, string? refreshToken);

    void Reset();
}
