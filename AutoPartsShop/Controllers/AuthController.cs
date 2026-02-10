using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Models.Dto;
using AutoPartsShop.Helpers; // Pentru PasswordHelper si TokenHelper
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AutoPartsShop.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

// ==================== WHOAMI ====================
[Authorize]
[HttpGet("me")]
public IActionResult Me()
{
    return Ok(new
    {
        Email = User.FindFirstValue(ClaimTypes.Name),
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        role = User.FindFirstValue(ClaimTypes.Role)
    });
}

// ==================== LOGIN ====================
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        return BadRequest("Emailul și parola sunt obligatorii");

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
        return Unauthorized("Email sau parola incorectă");

    if (user.IsDeleted)
        return Unauthorized("Cont șters. Contact suport"); //aici nu stiu ce sa zic daca las

    if (!user.IsActive)
        return Unauthorized("Cont dezactivat. Contacteaza suport");

    // ✅ Invalidate toate refresh token-urile vechi ale userului (1 sesiune activă)
    var oldTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && rt.Expires > DateTime.UtcNow)
        .ToListAsync();

    foreach (var t in oldTokens)
    {
        t.IsRevoked = true;
        t.RevokedAt = DateTime.UtcNow;
        t.RevokedByUserId = user.Id; // self (login revocă sesiunile vechi)
    }

    // Generare access token
    var accessToken = GenerateJwtToken(user);

    // Generare refresh token NOU
    var refreshToken = GenerateRefreshToken();

    var pepper = _config["Security:RefreshTokenPepper"];
    if (string.IsNullOrWhiteSpace(pepper))
        return StatusCode(500, "Security:RefreshTokenPepper lipsește din config.");

    var refreshTokenHash = TokenHelper.HashRefreshToken(refreshToken, pepper);

    _context.RefreshTokens.Add(new RefreshToken
    {
        TokenHash = refreshTokenHash,
        UserId = user.Id,
        Expires = DateTime.UtcNow.AddDays(7),
        IsRevoked = false
    });

    await _context.SaveChangesAsync();

    return Ok(new LoginResponseDto
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken
    });
}

    // ==================== REGISTER ====================
    [HttpPost("register")]
    public async Task<IActionResult> Register(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email și parola sunt obligatorii");

        var policy = PasswordPolicy.Validate(request.Password);
        if (!policy.IsValid)
            return BadRequest(policy.Error);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email deja folosit");

        var user = new User
        {
            Email = request.Email,
            Password = PasswordHelper.HashPassword(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User creat cu succes" });
    }

// ==================== REFRESH TOKEN ====================
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
{
    if (string.IsNullOrEmpty(request.RefreshToken))
        return BadRequest("Refresh token este obligatoriu");

    // 0) Calculăm hash-ul tokenului primit (TREBUIE înainte de query)
    var pepper = _config["Security:RefreshTokenPepper"];
    if (string.IsNullOrWhiteSpace(pepper))
        return StatusCode(500, "Security:RefreshTokenPepper lipsește din config.");

    var refreshTokenHash = TokenHelper.HashRefreshToken(request.RefreshToken, pepper);

    // 1) Căutăm token-ul în DB după hash
    var tokenEntity = await _context.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

    if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.Expires < DateTime.UtcNow)
        return Unauthorized("Refresh token invalid sau expirat");

    // BLOCK multiple active refresh tokens (SECURITY)
    var hasAnotherActiveToken = await _context.RefreshTokens.AnyAsync(rt =>
        rt.UserId == tokenEntity.UserId &&
        !rt.IsRevoked &&
        rt.Id != tokenEntity.Id
    );

    if (hasAnotherActiveToken)
    {
        return Unauthorized("Multiple active refresh tokens detected.");
    }  

    if (tokenEntity.User.IsDeleted)
        return Unauthorized("Cont șters");

    if (!tokenEntity.User.IsActive)
        return Unauthorized("Cont dezactivat");

    // 2) Generăm access token nou
    var newAccessToken = GenerateJwtToken(tokenEntity.User);

    // 3) ROTATION: revocăm tokenul vechi (cel folosit acum)
    tokenEntity.IsRevoked = true;
    tokenEntity.RevokedAt = DateTime.UtcNow;
    tokenEntity.RevokedByUserId = tokenEntity.UserId; // “self” (userul a rotit tokenul)

    // 4) Generăm refresh token nou și îl salvăm (hash în DB)
    var newRefreshToken = GenerateRefreshToken();
    var newRefreshTokenHash = TokenHelper.HashRefreshToken(newRefreshToken, pepper);

    _context.RefreshTokens.Add(new RefreshToken
    {
        TokenHash = newRefreshTokenHash,
        UserId = tokenEntity.UserId,
        Expires = DateTime.UtcNow.AddDays(7),
        IsRevoked = false
    });

    await _context.SaveChangesAsync();

    return Ok(new LoginResponseDto
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken
    });
}

// ==================== LOGOUT ====================
[Authorize]
[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
        return BadRequest("Refresh token este obligatoriu");

    var callerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(callerIdStr, out var callerId))
        return Unauthorized();

    var pepper = _config["Security:RefreshTokenPepper"];
    if (string.IsNullOrWhiteSpace(pepper))
        return StatusCode(500, "Security:RefreshTokenPepper lipsește din config.");

    var refreshTokenHash = TokenHelper.HashRefreshToken(request.RefreshToken, pepper);

    var tokenEntity = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

    if (tokenEntity == null)
        return Ok(new { message = "Logout realizat (token inexistent / deja invalid)" });

    // refresh token-ul trebuie să fie al userului logat
    if (tokenEntity.UserId != callerId)
        return Unauthorized("Refresh token nu aparține userului logat.");

    // dacă era deja revocat, nu mai rescriem audit-ul
    if (tokenEntity.IsRevoked || tokenEntity.RevokedAt != null)
        return Ok(new { message = "Logout realizat (token deja revocat)" });

    tokenEntity.IsRevoked = true;
    tokenEntity.RevokedAt = DateTime.UtcNow;
    tokenEntity.RevokedByUserId = callerId;

    await _context.SaveChangesAsync();

    return Ok(new { message = "Logout realizat cu succes. Refresh token revocat." });
}

// ==================== SELF: FORGOT PASSWORD TOKEN ====================
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
{
    // răspuns generic (anti user enumeration)
    var genericResponse = Ok(new { message = "Dacă email-ul există, ți-am trimis instrucțiuni de resetare." });

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user == null || user.IsDeleted || !user.IsActive)
        return genericResponse;

    //ștergem tokenuri vechi nefolosite pentru user
    var old = _context.PasswordResetTokens.Where(t => t.UserId == user.Id && t.UsedAt == null && !t.IsRevoked);
    foreach (var t in old)
    {
        t.IsRevoked = true;
    }

    // generează token (în clar) - de trimis pe email în producție
    var tokenBytes = RandomNumberGenerator.GetBytes(64);
    var token = Convert.ToBase64String(tokenBytes);

    var tokenHash = TokenHelper.HashToken(token);

    _context.PasswordResetTokens.Add(new PasswordResetToken
    {
        UserId = user.Id,
        TokenHash = tokenHash,
        ExpiresAt = DateTime.UtcNow.AddMinutes(15)
    });

    await _context.SaveChangesAsync();

    // DEV ONLY: returnăm tokenul ca să pot testa în Swagger.
    // În producție: nu-l returnez, îl trimit pe email.
    return Ok(new
    {
        message = "Token generat (DEV). În producție o sa trimit pe email.",
        resetToken = token,
        expiresAt = DateTime.UtcNow.AddMinutes(15)
    });
}

// ==================== SELF: RESET PASSWORD ====================
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        return BadRequest("Token și parolă nouă sunt obligatorii");

    var tokenHash = TokenHelper.HashToken(request.Token);

    var tokenEntity = await _context.PasswordResetTokens
        .Include(t => t.User)
        .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    if (tokenEntity == null ||
        tokenEntity.IsRevoked ||
        tokenEntity.UsedAt != null ||
        tokenEntity.ExpiresAt < DateTime.UtcNow ||
        tokenEntity.User.IsDeleted ||
        !tokenEntity.User.IsActive)
    {
        return Unauthorized("Token invalid sau expirat");
    }

    // validare complexitate parolă
    var policy = PasswordPolicy.Validate(request.NewPassword);
    if (!policy.IsValid)
        return BadRequest(policy.Error);

    // nu permite aceeași parolă ca înainte
    if (PasswordHelper.VerifyPassword(request.NewPassword, tokenEntity.User.Password))
        return BadRequest("Noua parolă nu poate fi aceeași ca parola veche.");

    //setează parola nouă
    tokenEntity.User.Password = PasswordHelper.HashPassword(request.NewPassword);

    //marchează token folosit
    tokenEntity.UsedAt = DateTime.UtcNow;

    //revocă refresh tokens (logout peste tot) + audit
    var userId = tokenEntity.UserId;

    var activeRefreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && !rt.IsRevoked)
        .ToListAsync();

    foreach (var rt in activeRefreshTokens)
    {
        rt.IsRevoked = true;
        rt.RevokedAt = DateTime.UtcNow;
        rt.RevokedByUserId = userId; // self
    }

    await _context.SaveChangesAsync();

    return Ok(new { message = "Parola a fost resetată cu succes. Te poți loga din nou." });
}

// ==================== SELF: DEACTIVATE ====================
[Authorize]
[HttpPost("deactivate")]
public async Task<IActionResult> Deactivate()
{
    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdStr, out var userId))
        return Unauthorized();

    var user = await _context.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    if (user.Role == "Admin")
    return BadRequest("Nu poți dezactiva contul de admin.");

    if (user.IsDeleted)
        return BadRequest("Contul este deja șters.");

    user.IsActive = false;

    // revoke toate refresh tokens ale userului
    var tokens = await _context.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
    foreach (var t in tokens) t.IsRevoked = true;

    await _context.SaveChangesAsync();

    return Ok(new { message = "Cont dezactivat." });
}

// ==================== SELF: SOFT DELETE ====================
[Authorize]
[HttpDelete("delete")]
public async Task<IActionResult> DeleteMe()
{
    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdStr, out var userId))
        return Unauthorized();

    var user = await _context.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    // opțional: nu lăsa admin să se șteargă singur prin endpoint-ul de self
 if (user.Role == "Admin")
    return BadRequest("Nu poți șters contul de admin.");

    user.IsDeleted = true;
    user.IsActive = false;

    // revoke toate refresh tokens ale userului
    var tokens = await _context.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
    foreach (var t in tokens) t.IsRevoked = true;

    await _context.SaveChangesAsync();

    return Ok(new { message = "Cont șters (soft delete)." });
}

    // ==================== HELPER METHODS ====================
    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey1234567890123456");
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddSeconds(30), //AddSeconds <-- 1 minut pentru test
            Issuer = _config["Jwt:Issuer"] ?? "MyIssuer",
            Audience = _config["Jwt:Audience"] ?? "MyAudience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
