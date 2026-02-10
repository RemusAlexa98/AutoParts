namespace AutoPartsShop.Helpers;

public static class PasswordHelper
{
    private const int WorkFactor = 11;

    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}