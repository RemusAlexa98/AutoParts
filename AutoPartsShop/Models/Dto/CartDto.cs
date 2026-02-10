namespace AutoPartsShop.Models.Dto;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();

    // total bani
    public decimal Total { get; set; }

    // total produse (sumă cantități)
    public int TotalItems { get; set; }
}