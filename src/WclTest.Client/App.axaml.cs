using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using WclTest.Api.DependencyInjection;
using WclTest.Client.Infrastructure;
using WclTest.Client.Services;
using WclTest.Client.ViewModels;
using WclTest.Shared;
using WclTest.Shared.Services;
#if FAKE_API
using WclTest.Api.Fake.DependencyInjection;
#endif

namespace WclTest.Client;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureLogging();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
            desktop.Exit += (_, _) => Log.CloseAndFlush();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureLogging()
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WclTest", "logs");
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDirectory, "wcltest-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AuthViewModel>();
        services.AddSingleton<UserViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IImageLoader, ImageLoader>();

#if FAKE_API
        services.AddWclApiFake();
#else
        services.AddWclApi(Constants.DefaultBaseUrl);
#endif
    }
}
