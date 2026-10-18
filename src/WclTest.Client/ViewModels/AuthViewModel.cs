using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WclTest.Client.Infrastructure;
using WclTest.Shared;
using WclTest.Shared.Services;

namespace WclTest.Client.ViewModels;

public partial class AuthViewModel : ViewModelBase
{
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    public AuthViewModel(IAuthService authService, IDialogService dialog)
    {
        AuthService = authService;
        _dialog = dialog;
    }

    public IAuthService AuthService { get; }

    [RelayCommand]
    private async Task LoginAsync(CancellationToken ct)
    {
        Error = string.Empty;
        try
        {
            await AuthService.LoginAsync(Email, Password, ct);
            Password = string.Empty;
        }
        catch (AuthException ex)
        {
            Error = ex.Message;
        }
        catch (OperationCanceledException)
        {
        }
    }

    [RelayCommand]
    private void CancelLogin() => LoginCommand.Cancel();

    [RelayCommand]
    private async Task LogoutAsync()
    {
        Error = string.Empty;
        await AuthService.LogoutAsync();
    }

    [RelayCommand]
    private Task RegisterAsync() => _dialog.ShowDialogAsync<RegisterViewModel>();
}
