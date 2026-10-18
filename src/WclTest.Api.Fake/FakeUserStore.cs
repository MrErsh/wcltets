using WclTest.Model.Auth;
using WclTest.Model.Users;
using WclTest.Shared;

namespace WclTest.Api.Fake;

internal sealed class FakeUserStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, StoredUser> _users = new();
    private readonly Dictionary<string, int> _access = new();
    private readonly Dictionary<string, int> _refresh = new();
    private int _nextId;

    public FakeUserStore() => AddUser("demo@example.com", "Passw0rd!", "Demo User");

    public UserDto Register(string email, string password, string name)
    {
        lock (_gate)
        {
            if (_users.ContainsKey(Normalize(email)))
                throw new AuthException("Пользователь с таким email уже существует");

            return ToDto(AddUser(email, password, name));
        }
    }

    public AuthResponse Login(string email, string password)
    {
        lock (_gate)
        {
            if (!_users.TryGetValue(Normalize(email), out var user) || user.Password != password)
                throw new AuthException("Неверный email или пароль");

            return Issue(user);
        }
    }

    public UserDto Me(string? accessToken)
    {
        lock (_gate)
        {
            if (accessToken is null || !_access.TryGetValue(accessToken, out var id))
                throw new AuthException("Сессия недействительна");

            return ToDto(_users.Values.First(u => u.Id == id));
        }
    }

    public void Logout(string? refreshToken)
    {
        lock (_gate)
        {
            if (refreshToken is null || !_refresh.Remove(refreshToken, out var id))
                return;

            foreach (var key in _access.Where(kv => kv.Value == id).Select(kv => kv.Key).ToList())
                _access.Remove(key);
        }
    }

    private StoredUser AddUser(string email, string password, string name)
    {
        var id = ++_nextId;
        var normalized = Normalize(email);
        var user = new StoredUser(
            id,
            normalized,
            password,
            string.IsNullOrWhiteSpace(name) ? normalized : name.Trim(),
            "user",
            null);
        _users[normalized] = user;
        return user;
    }

    private AuthResponse Issue(StoredUser user)
    {
        var access = "acc_" + Guid.NewGuid().ToString("N");
        var refresh = "rt_mock_" + Guid.NewGuid().ToString("N");
        _access[access] = user.Id;
        _refresh[refresh] = user.Id;
        return new AuthResponse(ToDto(user), access, refresh);
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static UserDto ToDto(StoredUser u) => new($"usr_{u.Id:D6}", u.Email, u.Name, u.Avatar, u.Role);
}

internal sealed record StoredUser(int Id, string Email, string Password, string Name, string Role, string? Avatar);
