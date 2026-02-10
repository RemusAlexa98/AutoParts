using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}