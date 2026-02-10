using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; } = 1;

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}