using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize] // doar user logat
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // helper: ia userId din JWT
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private async Task<CartDto> BuildCartDto(int userId)
    {
    var items = await _context.CartItems
        .Where(ci => ci.UserId == userId)
        .Include(ci => ci.Product)
        .Select(ci => new CartItemDto
        {
            Id = ci.Id,
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            Manufacturer = ci.Product.Manufacturer,
            Price = ci.Product.Price,
            Quantity = ci.Quantity
        })
        .ToListAsync();

    return new CartDto
    {
    Items = items,
    Total = items.Sum(i => i.LineTotal),
    TotalItems = items.Sum(i => i.Quantity)
    };
    }    

    // ==================== GET CART ====================
    [HttpGet]
public async Task<IActionResult> GetCart()
{
    var userId = GetUserId();
    var cart = await BuildCartDto(userId);
    return Ok(cart);
}

    // ==================== ADD TO CART ====================
[HttpPost("items")]
public async Task<IActionResult> AddToCart(AddToCartRequest request)
{
    var userId = GetUserId();

    var product = await _context.Products.FindAsync(request.ProductId);
    if (product == null)
        return NotFound("Produs inexistent");

    // ✅ Verificăm cât ai deja în coș la produsul ăsta
    var currentQtyInCart = await _context.CartItems
        .Where(ci => ci.UserId == userId && ci.ProductId == request.ProductId)
        .Select(ci => (int?)ci.Quantity)
        .FirstOrDefaultAsync() ?? 0;

    var requestedTotal = currentQtyInCart + request.Quantity;

    // ✅ Validare stoc (nu lăsăm userul să depășească stocul)
    if (requestedTotal > product.Stock)
    {
        return BadRequest(
            $"Stoc insuficient pentru {product.Name}. Disponibil: {product.Stock}, în coș ai: {currentQtyInCart}, încerci să ajungi la: {requestedTotal}"
        );
    }

    // Dacă există deja item-ul, creștem cantitatea
    var existingItem = await _context.CartItems
        .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

    if (existingItem != null)
    {
        existingItem.Quantity += request.Quantity;
    }
    else
    {
        _context.CartItems.Add(new CartItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        });
    }

    await _context.SaveChangesAsync();
    var cart = await BuildCartDto(userId);
    return Ok(cart);
}

// ==================== UPDATE CART ITEM ====================
[HttpPut("items/{id}")]
public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemRequest request)
{
    var userId = GetUserId();

    var item = await _context.CartItems
        .Include(ci => ci.Product)
        .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

    if (item == null)
        return NotFound("Item inexistent");

    // ✅ Validare stoc
    if (request.Quantity > item.Product.Stock)
    {
        return BadRequest(
            $"Stoc insuficient pentru {item.Product.Name}. Disponibil: {item.Product.Stock}, cerut: {request.Quantity}"
        );
    }

    item.Quantity = request.Quantity;
    await _context.SaveChangesAsync();

    var cart = await BuildCartDto(userId);
    return Ok(cart);
}

    // ==================== REMOVE FROM CART ====================
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> RemoveCartItem(int id)
    {
        var userId = GetUserId();

        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

        if (item == null)
            return NotFound("Item inexistent");

    _context.CartItems.Remove(item);
    await _context.SaveChangesAsync();

    var cart = await BuildCartDto(userId);
    return Ok(cart);
    }
}
