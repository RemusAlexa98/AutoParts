using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Models.Dto;

public class UpdateCartItemRequest
{
    [Range(1, 999)]
    public int Quantity { get; set; }
}