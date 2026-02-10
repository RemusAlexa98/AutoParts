namespace AutoPartsShop.Models.Dto;

public class CancelOrderResponseDto
{
    public string Message { get; set; } = "Comandă anulată";
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}