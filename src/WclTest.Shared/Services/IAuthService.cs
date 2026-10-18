using System.ComponentModel;
using WclTest.Model.Users;

namespace WclTest.Shared.Services;

public interface IAuthService : INotifyPropertyChanged
{
    bool IsAuthenticated { get; }

    UserDto? CurrentUser { get; }

    Task LoginAsync(string email, string password, CancellationToken ct = default);

    Task RegisterAsync(string email, string name, string password, CancellationToken ct = default);

    Task LogoutAsync(CancellationToken ct = default);
}
