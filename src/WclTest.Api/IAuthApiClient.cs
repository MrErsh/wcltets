using Refit;
using WclTest.Model.Auth;
using WclTest.Model.Users;

namespace WclTest.Api;

public interface IAuthApiClient
{
    [Post("/auth/register")]
    Task<UserDto> RegisterAsync([Body] RegisterRequest request, CancellationToken ct = default);

    [Post("/auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request, CancellationToken ct = default);

    [Get("/auth/me")]
    Task<UserDto> MeAsync(CancellationToken ct = default);

    [Post("/auth/logout")]
    Task LogoutAsync([Body] LogoutRequest request, CancellationToken ct = default);
}
