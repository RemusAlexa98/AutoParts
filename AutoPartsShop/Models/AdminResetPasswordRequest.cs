using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class AdminResetPasswordRequest
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}