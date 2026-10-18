using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Text.Json;
using WclTest.Api.Http;
using WclTest.Shared.Services;

namespace WclTest.Api.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddWclApi(this IServiceCollection services, string baseUrl)
    {
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddTransient<AuthHeaderHandler>();

        var json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
        };
        var settings = new RefitSettings(new SystemTextJsonContentSerializer(json));

        services.AddRefitClient<IAuthApiClient>(settings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddStandardResilienceHandler();

        return services;
    }
}
