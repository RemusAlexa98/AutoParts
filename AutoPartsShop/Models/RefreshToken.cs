using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPartsShop.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }  // <-- schimbat la int

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    public DateTime Expires { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public int? RevokedByUserId { get; set; }  // cine a revocat tokenul

    [ForeignKey("RevokedByUserId")]
    public User? RevokedBy { get; set; }
}
