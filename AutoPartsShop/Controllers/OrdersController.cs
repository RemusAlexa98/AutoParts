using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize] // user logat
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }


 // ==================== LISTA COMENZI USER ====================
  // GET: /api/orders?status=
[HttpGet]
public async Task<IActionResult> GetMyOrders(
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string sort = "createdAtDesc")
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var userId = GetUserId();

    var query = _context.Orders
        .Where(o => o.UserId == userId)
        .Include(o => o.Items).ThenInclude(i => i.Product)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(o => o.Status == status);

    query = sort == "createdAtAsc"
        ? query.OrderBy(o => o.CreatedAt)
        : query.OrderByDescending(o => o.CreatedAt);

    var totalCount = await query.CountAsync();

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new OrderDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        })
        .ToListAsync();

    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return Ok(new
    {
        page,
        pageSize,
        totalCount,
        totalPages,
        items
    });
}


    // ==================== CHECKOUT ====================
[HttpPost]
public async Task<IActionResult> Checkout()
{
    var userId = GetUserId();

    await using var tx = await _context.Database.BeginTransactionAsync();

    var cartItems = await _context.CartItems
        .Where(ci => ci.UserId == userId)
        .Include(ci => ci.Product)
        .ToListAsync();

    if (!cartItems.Any())
        return BadRequest("Coșul este gol");

    var order = new Order
    {
        UserId = userId,
        Status = "Pending"
    };

    foreach (var item in cartItems)
    {
        // ✅ Scădere stoc ATOMICĂ în DB
        // Dacă altcineva a cumpărat între timp, update-ul va afecta 0 rânduri
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Products""
            SET ""Stock"" = ""Stock"" - {item.Quantity}
            WHERE ""Id"" = {item.ProductId} AND ""Stock"" >= {item.Quantity};
        ");

        if (rows == 0)
        {
            await tx.RollbackAsync();
            return BadRequest($"Stoc epuizat pentru {item.Product.Name}. Încearcă din nou.");
        }

        // înghețăm prețul la momentul comenzii
        order.Items.Add(new OrderItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.Product.Price
        });
    }

    _context.Orders.Add(order);

    // golim coșul
    _context.CartItems.RemoveRange(cartItems);

    await _context.SaveChangesAsync();
    await tx.CommitAsync();

    return Ok(new CheckoutResponseDto
    {
    Message = "Comandă creată",
    OrderId = order.Id
    });
}

    // ==================== CANCEL ORDER (doar dacă e Pending) ====================
[HttpPost("{id}/cancel")]
public async Task<IActionResult> CancelOrder(int id)
{
    var userId = GetUserId();

    await using var tx = await _context.Database.BeginTransactionAsync();

    var order = await _context.Orders
        .Include(o => o.Items)
        .ThenInclude(i => i.Product)
        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

    if (order == null)
        return NotFound("Comanda nu există");

    if (order.Status != "Pending")
        return BadRequest("Doar comenzile Pending pot fi anulate");

    order.Status = "Cancelled";

    // ✅ refacem stocul o singură dată
    if (!order.StockRestored)
    {
        foreach (var item in order.Items)
        {
            item.Product.Stock += item.Quantity;
        }
        order.StockRestored = true;
    }

    await _context.SaveChangesAsync();
    await tx.CommitAsync();

    return Ok(new CancelOrderResponseDto
    {
    Message = "Comandă anulată",
    Id = order.Id,
    Status = order.Status
    });
}
}
