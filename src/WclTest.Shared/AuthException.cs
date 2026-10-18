namespace WclTest.Shared;

public sealed class AuthException : Exception
{
    public AuthException(string message)
        : base(message)
    {
    }
}
