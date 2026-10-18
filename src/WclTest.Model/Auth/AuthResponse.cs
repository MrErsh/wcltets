using WclTest.Model.Users;

namespace WclTest.Model.Auth;

public sealed record AuthResponse(UserDto? User, string AccessToken, string RefreshToken);
