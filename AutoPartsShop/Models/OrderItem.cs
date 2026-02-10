using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; }

    // prețul “înghețat” la momentul comenzii
    [Range(0, 1_000_000)]
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}