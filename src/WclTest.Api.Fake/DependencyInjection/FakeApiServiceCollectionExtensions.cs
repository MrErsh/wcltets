using Microsoft.Extensions.DependencyInjection;
using WclTest.Shared.Services;

namespace WclTest.Api.Fake.DependencyInjection;

public static class FakeApiServiceCollectionExtensions
{
    public static IServiceCollection AddWclApiFake(this IServiceCollection services)
    {
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<IAuthApiClient, FakeAuthApiClient>();
        return services;
    }
}
