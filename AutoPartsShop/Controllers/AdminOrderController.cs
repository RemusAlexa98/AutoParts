using AutoPartsShop.Data;
using AutoPartsShop.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminOrdersController(AppDbContext context)
    {
        _context = context;
    }

    // ==================== LISTA TOATE COMENZILE ====================
// GET: /api/admin/orders?status=&userId=&Email=&page=&pageSize=&sort=
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string? status,
    [FromQuery] int? userId,
    [FromQuery] string? Email,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string sort = "createdAtDesc")
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var query = _context.Orders
        .Include(o => o.User)
        .Include(o => o.Items).ThenInclude(i => i.Product)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(o => o.Status == status);

    if (userId.HasValue)
        query = query.Where(o => o.UserId == userId.Value);

    if (!string.IsNullOrWhiteSpace(Email))
        query = query.Where(o => o.User.Email.ToLower().Contains(Email.ToLower()));

    query = sort == "createdAtAsc"
        ? query.OrderBy(o => o.CreatedAt)
        : query.OrderByDescending(o => o.CreatedAt);

    var totalCount = await query.CountAsync();

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new AdminOrderDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            UserId = o.UserId,
            Email = o.User.Email,
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

    // ==================== SCHIMBA STATUS ====================
    [HttpPut("{id}/status")]
public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
{
    var allowed = new[] { "Pending", "Paid", "Shipped", "Cancelled" };
    if (!allowed.Contains(request.Status))
        return BadRequest("Status invalid. Folosește: Pending, Paid, Shipped, Cancelled.");

    await using var tx = await _context.Database.BeginTransactionAsync();

    var order = await _context.Orders
        .Include(o => o.Items)
        .ThenInclude(i => i.Product)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
        return NotFound("Comanda nu există");

    // ✅ Nu permitem reactivarea comenzilor Cancelled (evită probleme și abuz)
    if (order.Status == "Cancelled" && request.Status != "Cancelled")
        return BadRequest("O comandă anulată (Cancelled) nu poate fi schimbată înapoi.");

    // ✅ Dacă trecem din Pending -> Cancelled, refacem stocul o singură dată
    if (order.Status != "Cancelled" && request.Status == "Cancelled")
    {
        if (!order.StockRestored)
        {
            foreach (var item in order.Items)
            {
                item.Product.Stock += item.Quantity;
            }
            order.StockRestored = true;
        }
    }

    order.Status = request.Status;

    await _context.SaveChangesAsync();
    await tx.CommitAsync();

    return Ok(new { message = "Status actualizat", order.Id, order.Status });
}
}
