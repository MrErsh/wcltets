using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Refit;
using WclTest.Api;
using WclTest.Model.Auth;
using WclTest.Model.Users;
using WclTest.Shared;
using WclTest.Shared.Services;

namespace WclTest.Client.Services;

public sealed partial class AuthService : ObservableObject, IAuthService
{
    private readonly IAuthApiClient _api;
    private readonly ITokenStorage _tokens;
    private readonly ILogger<AuthService> _logger;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private UserDto? _currentUser;

    public AuthService(IAuthApiClient api, ITokenStorage tokens, ILogger<AuthService> logger)
    {
        _api = api;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await RunAsync(c => _api.LoginAsync(new LoginRequest(email, password), c), ct);
        _tokens.Set(response.AccessToken, response.RefreshToken);
        CurrentUser = await RunAsync(c => _api.MeAsync(c), ct);
        IsAuthenticated = true;
        _logger.LogInformation("User {Email} signed in", email);
    }

    public async Task RegisterAsync(string email, string name, string password, CancellationToken ct = default)
    {
        await RunAsync(c => _api.RegisterAsync(new RegisterRequest(email, password, name), c), ct);
        var auth = await RunAsync(c => _api.LoginAsync(new LoginRequest(email, password), c), ct);
        _tokens.Set(auth.AccessToken, auth.RefreshToken);
        CurrentUser = await RunAsync(c => _api.MeAsync(c), ct);
        IsAuthenticated = true;
        _logger.LogInformation("User {Email} registered", email);
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            await _api.LogoutAsync(new LogoutRequest(_tokens.RefreshToken), ct);
        }
        catch (Exception ex) when (ex is ApiException || IsTransport(ex))
        {
            _logger.LogWarning(ex, "Logout request failed; clearing local session anyway");
        }
        finally
        {
            _tokens.Reset();
            CurrentUser = null;
            IsAuthenticated = false;
        }
    }

    private async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> call, CancellationToken ct)
    {
        try
        {
            return await call(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API returned {Status}", (int)ex.StatusCode);
            throw new AuthException(Describe(ex));
        }
        catch (Exception ex) when (IsTransport(ex))
        {
            _logger.LogWarning(ex, "API transport failure");
            throw new AuthException("Сервис временно недоступен. Попробуйте позже.");
        }
    }

    private static bool IsTransport(Exception ex) =>
        ex is HttpRequestException
            or BrokenCircuitException
            or ExecutionRejectedException
            or OperationCanceledException;

    private static string Describe(ApiException ex) => ex.StatusCode switch
    {
        HttpStatusCode.Unauthorized => "Неверный email или пароль",
        HttpStatusCode.BadRequest => "Некорректные данные",
        _ => $"Ошибка сервера ({(int)ex.StatusCode})",
    };
}
