using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using WclTest.Client.ViewModels;

namespace WclTest.Client.Infrastructure;

public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _services;
    private readonly ViewLocator _viewLocator = new();

    public DialogService(IServiceProvider services) => _services = services;

    public async Task<bool> ShowDialogAsync<TViewModel>() where TViewModel : ViewModelBase, IDialogViewModel
    {
        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
            ?? throw new InvalidOperationException("Main window is not available.");

        var vm = _services.GetRequiredService<TViewModel>();
        var content = _viewLocator.Build(vm)
            ?? throw new InvalidOperationException($"No view registered for {typeof(TViewModel).Name}.");
        content.DataContext = vm;

        var result = false;
        var window = new Window
        {
            Title = vm.Title,
            Content = content,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        vm.CloseRequested = closed =>
        {
            result = closed;
            window.Close();
        };

        try
        {
            await window.ShowDialog(owner);
        }
        finally
        {
            vm.CloseRequested = null;
        }

        return result;
    }
}
