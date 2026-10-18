namespace WclTest.Model.Users;

public sealed record UserDto(string Id, string Email, string Name, string? Avatar, string? Role);
