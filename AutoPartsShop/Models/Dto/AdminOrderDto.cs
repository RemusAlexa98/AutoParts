namespace AutoPartsShop.Models.Dto;

public class AdminOrderDto : OrderDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}