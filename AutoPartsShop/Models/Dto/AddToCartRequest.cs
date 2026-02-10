using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models.Dto;

public class AddToCartRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; } = 1;
}