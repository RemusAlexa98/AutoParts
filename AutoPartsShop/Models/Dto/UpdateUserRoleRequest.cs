namespace AutoPartsShop.Models.Dto;

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = "User"; // "Admin" sau "User"
}