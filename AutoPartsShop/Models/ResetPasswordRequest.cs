using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}