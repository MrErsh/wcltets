using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WclTest.Client.Infrastructure;
using WclTest.Shared;
using WclTest.Shared.Services;

namespace WclTest.Client.ViewModels;

public partial class RegisterViewModel : ViewModelBase, IDialogViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    public RegisterViewModel(IAuthService auth) => _auth = auth;

    public string Title => "Регистрация";

    public Action<bool>? CloseRequested { get; set; }

    [RelayCommand]
    private async Task RegisterAsync(CancellationToken ct)
    {
        Error = string.Empty;
        try
        {
            await _auth.RegisterAsync(Email, Name, Password, ct);
            CloseRequested?.Invoke(true);
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
    private void CancelRegister() => RegisterCommand.Cancel();

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
