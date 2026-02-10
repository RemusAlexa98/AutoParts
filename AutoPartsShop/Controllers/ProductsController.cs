using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

// ==================== GET PRODUCTS ====================
    // GET: /api/products - acces public pentru toti userii
 [HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string? name,
    [FromQuery] string? manufacturer,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var query = _context.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(name))
        query = query.Where(p => p.Name.Contains(name));

    if (!string.IsNullOrWhiteSpace(manufacturer))
        query = query.Where(p => p.Manufacturer.Contains(manufacturer));

    var totalCount = await query.CountAsync();

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Manufacturer = p.Manufacturer,
            Price = p.Price,
            Stock = p.Stock
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

// ==================== CREATE ====================
    // POST: /api/admin/products - doar Admin
[Authorize(Roles = "Admin")]
[HttpPost("/api/admin/products")]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    var product = new Product
    {
        Name = dto.Name,
        Manufacturer = dto.Manufacturer,
        Price = dto.Price,
        Stock = dto.Stock
    };

    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    var result = new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Manufacturer = product.Manufacturer,
        Price = product.Price,
        Stock = product.Stock
    };

    return Ok(result);
}

// ==================== UPDATE ====================
// PUT: /api/admin/products/{id} - doar Admin
[Authorize(Roles = "Admin")]
[HttpPut("/api/admin/products/{id}")]
public async Task<IActionResult> Update(int id, UpdateProductDto dto)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return NotFound();

    product.Name = dto.Name;
    product.Manufacturer = dto.Manufacturer;
    product.Price = dto.Price;
    product.Stock = dto.Stock;

    await _context.SaveChangesAsync();

    var result = new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Manufacturer = product.Manufacturer,
        Price = product.Price,
        Stock = product.Stock
    };

    return Ok(result);
}

// ==================== DELETE ====================
// DELETE: /api/admin/products/{id} - doar Admin
[Authorize(Roles = "Admin")]
[HttpDelete("/api/admin/products/{id}")]
public async Task<IActionResult> Delete(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return NotFound();

    _context.Products.Remove(product);
    await _context.SaveChangesAsync();
    return NoContent();
}

}
