namespace AutoPartsShop.Models.Dto;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}