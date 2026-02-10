using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models; // sau Dtos, unde îl ții tu

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}