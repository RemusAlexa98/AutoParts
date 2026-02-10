using System.Security.Cryptography;
using System.Text;

namespace AutoPartsShop.Helpers;

public static class TokenHelper
{
    // Folosit la reset-password tokens (ok să rămână)
    public static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    // Folosit la refresh tokens (mai sigur): HMACSHA256 + pepper
    public static string HashRefreshToken(string token, string pepper)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.", nameof(token));
        if (string.IsNullOrWhiteSpace(pepper))
            throw new ArgumentException("Pepper is required.", nameof(pepper));

        var keyBytes = Encoding.UTF8.GetBytes(pepper);
        using var hmac = new HMACSHA256(keyBytes);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = hmac.ComputeHash(tokenBytes);

        return Convert.ToBase64String(hash);
    }
}
