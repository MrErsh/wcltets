using WclTest.Model.Auth;
using WclTest.Model.Users;
using WclTest.Shared.Services;

namespace WclTest.Api.Fake;

public sealed class FakeAuthApiClient : IAuthApiClient
{
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(1000);
    private readonly FakeUserStore _store = new();
    private readonly ITokenStorage _tokens;

    public FakeAuthApiClient(ITokenStorage tokens) => _tokens = tokens;

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _store.Register(request.Email, request.Password, request.Name);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _store.Login(request.Email, request.Password);
    }

    public async Task<UserDto> MeAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _store.Me(_tokens.AccessToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        _store.Logout(request.RefreshToken);
    }
}
