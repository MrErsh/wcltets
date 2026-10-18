using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Refit;
using System.Net;
using System.Reflection;
using WclTest.Api;
using WclTest.Client.Services;
using WclTest.Model.Auth;
using WclTest.Model.Users;
using WclTest.Shared;

namespace WclTest.Tests;

public sealed class AuthServiceTests
{
    private static readonly UserDto DemoUser =
        new("usr_000001", "demo@example.com", "Demo User", "https://avatar/1", "user");

    private static AuthResponse Response() => new(DemoUser, "acc_1", "ref_1");

    private static ApiException ApiError(HttpStatusCode code)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api/auth/login");
        var response = new HttpResponseMessage(code);
        var ctor = typeof(ApiException)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(c => c.GetParameters().Length == 7);
        return (ApiException)ctor.Invoke(
        [
            request, request.Method, string.Empty, code, "error", response.Headers, new RefitSettings(),
        ]);
    }

    [Fact]
    public async Task Login_Success_SetsTokensAndHydratesProfileViaMe()
    {
        var api = new Mock<IAuthApiClient>();
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response());
        api.Setup(a => a.MeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(DemoUser);
        var tokens = new TokenStorage();
        var sut = new AuthService(api.Object, tokens, NullLogger<AuthService>.Instance);

        await sut.LoginAsync("demo@example.com", "Passw0rd!");

        sut.IsAuthenticated.Should().BeTrue();
        sut.CurrentUser.Should().Be(DemoUser);
        tokens.AccessToken.Should().Be("acc_1");
        tokens.RefreshToken.Should().Be("ref_1");
        api.Verify(a => a.MeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsAuthException_AndKeepsSignedOut()
    {
        var api = new Mock<IAuthApiClient>();
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ApiError(HttpStatusCode.Unauthorized));
        var sut = new AuthService(api.Object, new TokenStorage(), NullLogger<AuthService>.Instance);

        var act = () => sut.LoginAsync("demo@example.com", "wrong");

        await act.Should().ThrowAsync<AuthException>();
        sut.IsAuthenticated.Should().BeFalse();
        sut.CurrentUser.Should().BeNull();
        api.Verify(a => a.MeAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Cancelled_PropagatesOperationCanceled()
    {
        var api = new Mock<IAuthApiClient>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));
        var sut = new AuthService(api.Object, new TokenStorage(), NullLogger<AuthService>.Instance);

        var act = () => sut.LoginAsync("demo@example.com", "Passw0rd!", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        sut.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task Register_Success_AutoLogsInViaLoginThenMe()
    {
        var api = new Mock<IAuthApiClient>();
        api.Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(DemoUser);
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response());
        api.Setup(a => a.MeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(DemoUser);
        var sut = new AuthService(api.Object, new TokenStorage(), NullLogger<AuthService>.Instance);

        await sut.RegisterAsync("demo@example.com", "Demo User", "Passw0rd!");

        sut.IsAuthenticated.Should().BeTrue();
        sut.CurrentUser.Should().Be(DemoUser);
        api.Verify(a => a.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        api.Verify(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        api.Verify(a => a.MeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_ClearsSessionAndTokens()
    {
        var api = new Mock<IAuthApiClient>();
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response());
        api.Setup(a => a.MeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(DemoUser);
        api.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var tokens = new TokenStorage();
        var sut = new AuthService(api.Object, tokens, NullLogger<AuthService>.Instance);
        await sut.LoginAsync("demo@example.com", "Passw0rd!");

        await sut.LogoutAsync();

        sut.IsAuthenticated.Should().BeFalse();
        sut.CurrentUser.Should().BeNull();
        tokens.AccessToken.Should().BeNull();
        tokens.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Logout_ClearsSession_EvenWhenApiFails()
    {
        var api = new Mock<IAuthApiClient>();
        api.Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response());
        api.Setup(a => a.MeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(DemoUser);
        api.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ApiError(HttpStatusCode.Unauthorized));
        var tokens = new TokenStorage();
        var sut = new AuthService(api.Object, tokens, NullLogger<AuthService>.Instance);
        await sut.LoginAsync("demo@example.com", "Passw0rd!");

        await sut.LogoutAsync();

        sut.IsAuthenticated.Should().BeFalse();
        tokens.AccessToken.Should().BeNull();
    }
}
