using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // "Pending", "Paid", "Shipped", "Cancelled" etc.
    public string Status { get; set; } = "Pending";

    public User User { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
     public bool StockRestored { get; set; } = false;     /// True dacă stocul a fost refăcut deja pentru comanda asta (la cancel). Protecție anti-dublu-restock.
}