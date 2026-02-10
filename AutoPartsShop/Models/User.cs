using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!; // hash-ul parolei

    [Required]
    public string Role { get; set; } = "User"; // Admin / User

    public bool IsActive { get; set; } = true;   // activ/dezactivat de admin sau user
    public bool IsDeleted { get; set; } = false; // soft delete
}