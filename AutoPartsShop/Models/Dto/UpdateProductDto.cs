namespace AutoPartsShop.Models.Dto;

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}