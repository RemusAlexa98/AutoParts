using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Manufacturer { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}
