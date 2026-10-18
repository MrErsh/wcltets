using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using WclTest.Client.Services;
using WclTest.Shared.Services;

namespace WclTest.Client.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly IImageLoader _imageLoader;

    [ObservableProperty]
    private Bitmap? _avatar;

    public UserViewModel(IAuthService authService, IImageLoader imageLoader)
    {
        AuthService = authService;
        _imageLoader = imageLoader;
        AuthService.PropertyChanged += OnAuthPropertyChanged;
    }

    public IAuthService AuthService { get; }

    public string Initials =>
        AuthService.CurrentUser?.Name is { Length: > 0 } name ? name[..1].ToUpperInvariant() : string.Empty;

    private void OnAuthPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAuthService.CurrentUser))
            _ = LoadAvatarAsync();
    }

    private async Task LoadAvatarAsync()
    {
        var user = AuthService.CurrentUser;
        OnPropertyChanged(nameof(Initials));

        var loaded = user is null ? null : await _imageLoader.LoadAsync(user.Avatar);
        var previous = Avatar;
        Avatar = loaded;
        previous?.Dispose();
    }
}
