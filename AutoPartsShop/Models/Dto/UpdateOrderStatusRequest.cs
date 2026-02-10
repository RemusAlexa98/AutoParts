namespace AutoPartsShop.Models.Dto;

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = "Pending"; 
    // ex: "Pending", "Paid", "Shipped", "Cancelled"
}