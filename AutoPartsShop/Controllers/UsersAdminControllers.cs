using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Models.Dto;
using AutoPartsShop.Helpers; // Pentru PasswordHelper si TokenHelper
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersAdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersAdminController(AppDbContext context)
    {
        _context = context;
    }

// ==================== ADMIN: USERS ====================
    // GET: /api/admin/users
    [HttpGet]
public async Task<IActionResult> GetUsers(
    [FromQuery] string? role,
    [FromQuery] bool? isActive,
    [FromQuery] bool? isDeleted,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var query = _context.Users.AsQueryable();

    if (!string.IsNullOrWhiteSpace(role))
        query = query.Where(u => u.Role == role);

    if (isActive.HasValue)
        query = query.Where(u => u.IsActive == isActive.Value);

    if (isDeleted.HasValue)
        query = query.Where(u => u.IsDeleted == isDeleted.Value);

     var totalCount = await query.CountAsync();

    var item = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            IsDeleted = u.IsDeleted
        })
        .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

       return Ok(new
    {
        page,
        pageSize,
        totalCount,
        totalPages,
        item
    });
}

// ==================== ADMIN: ROLE ====================
    // PUT: /api/admin/users/{id}/role
[HttpPut("{id}/role")]
public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
{
    if (request.Role != "Admin" && request.Role != "User")
        return BadRequest("Rol invalid. Folosește 'Admin' sau 'User'.");

    var user = await _context.Users.FindAsync(id);
    if (user == null)
        return NotFound("User inexistent");

    // Nu permitem schimbarea rolului pentru adminul principal
    if (user.Email == "admin@autoparts.local")
        return BadRequest("Rolul adminului principal nu poate fi schimbat");

    if (user.IsDeleted)
    return BadRequest("Cont șters. Nu se poate schimba rolul.");

    user.Role = request.Role;
    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Rol actualizat",
        user.Id,
        user.Email,
        user.Role
    });
}

// ==================== ADMIN: ACTIVATE ====================
// PUT: /api/admin/users/{id}/active
[HttpPut("{id}/active")]
public async Task<IActionResult> SetActive(int id, SetUserActiveRequest request)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null)
        return NotFound("User inexistent");

    // Nu permitem dezactivarea adminului principal
if (user.Role == "Admin")
    return BadRequest("Nu poți dezactiva contul de admin.");

    if (user.IsDeleted)
    return BadRequest("Cont șters. Nu poate fi reactivat.");

    user.IsActive = request.IsActive;
    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Status actualizat",
        user.Id,
        user.Email,
        user.IsActive
    });
}

// ==================== ADMIN: RESET PASSWORD ====================
[Authorize(Roles = "Admin")]
[HttpPut("{id}/password-reset")]
public async Task<IActionResult> AdminResetPassword(int id, AdminResetPasswordRequest request)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user == null) return NotFound();

    if (user.IsDeleted) return BadRequest("Contul este șters.");

    // Blochează adminul să-și reseteze propria parolă din endpoint-ul de admin
    var callerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(callerIdStr, out var callerId))
    return Unauthorized();

    if (callerId == id)
    return BadRequest("Nu îți poți reseta propria parolă din endpoint-ul de admin. Folosește /api/auth/change-password sau /api/auth/reset-password.");
 

    user.Password = PasswordHelper.HashPassword(request.NewPassword);

    // revocă refresh tokens
    var tokens = _context.RefreshTokens.Where(rt => rt.UserId == id && !rt.IsRevoked);
    await tokens.ForEachAsync(rt => rt.IsRevoked = true);

    await _context.SaveChangesAsync();

    return Ok(new { message = "Parola a fost resetată." });
}

// ==================== ADMIN: SOFT DELETE USER ====================
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();

    // nu lăsa admin să se șteargă singur
    if (user.Role == "Admin")
    return BadRequest("Nu poți șters contul de admin.");

    user.IsDeleted = true;
    user.IsActive = false;

    // revoke toate refresh tokens ale userului
    var tokens = await _context.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
    foreach (var t in tokens) t.IsRevoked = true;

    await _context.SaveChangesAsync();

    return NoContent();
}
}