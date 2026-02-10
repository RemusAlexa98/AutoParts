namespace AutoPartsShop.Models.Dto;

public class CheckoutResponseDto
{
    public string Message { get; set; } = "Comandă creată";
    public int OrderId { get; set; }
}