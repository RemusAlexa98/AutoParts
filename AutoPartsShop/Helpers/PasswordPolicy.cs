using System.Text.RegularExpressions;

namespace AutoPartsShop.Helpers;

public static class PasswordPolicy
{
    public static (bool IsValid, string Error) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Parola este obligatorie.");

        if (password.Length < 8)
            return (false, "Parola trebuie să aibă minimum 8 caractere.");

        if (!Regex.IsMatch(password, "[A-Z]"))
            return (false, "Parola trebuie să conțină cel puțin o literă mare.");

        if (!Regex.IsMatch(password, "[0-9]"))
            return (false, "Parola trebuie să conțină cel puțin o cifră.");

        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return (false, "Parola trebuie să conțină cel puțin un caracter special.");

        return (true, string.Empty);
    }
}