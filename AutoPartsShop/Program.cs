using AutoPartsShop.Data;
using AutoPartsShop.Models;
using AutoPartsShop.Helpers; // PasswordHelper
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ==================== Configurare DbContext ====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ==================== CORS (pentru UI React) ====================
// - Vite (cel mai popular acum) rulează pe 5173
// - Create React App rulează pe 3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("UI", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // .AllowCredentials(); // folosește doar dacă UI trimite cookies; la JWT în header nu e necesar
    });
});

// ==================== Configurare JWT ====================
var keyString = builder.Configuration["Jwt:Key"] ?? "SuperSecretKey1234567890123456"; // key >=256bit
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyIssuer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MyAudience",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Extra safety: dacă userul e șters/dezactivat, invalidăm tokenul
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdStr = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                context.Fail("Invalid token.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                context.Fail("Account disabled or deleted.");
                return;
            }
        }
    };
});

// ==================== Authorization ====================
builder.Services.AddAuthorization();

// ==================== Controllers + Swagger ====================
builder.Services.AddControllers();

// ProblemDetails = format standard JSON pentru erori
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoPartsShop API", Version = "v1" });

    // Config JWT în Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Add just the JWT token, without 'Bearer'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ==================== IP Rate Limiting ====================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// ==================== Handler global de erori (JSON) ====================
// - orice excepție necapturată => 500 + ProblemDetails JSON
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionFeature?.Error;

        var problem = new ProblemDetails
        {
            Title = "A apărut o eroare pe server.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = app.Environment.IsDevelopment()
                ? ex?.Message
                : "Încearcă din nou mai târziu."
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

// - pentru 404/401/403 întoarce JSON (nu HTML)
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;

    if (response.HasStarted) return;

    if (response.StatusCode is 404 or 401 or 403)
    {
        response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Status = response.StatusCode,
            Detail = response.StatusCode switch
            {
                404 => "Resource not found.",
                401 => "Unauthorized.",
                403 => "Forbidden.",
                _ => "Request failed."
            }
        };

        await response.WriteAsJsonAsync(problem);
    }
});

// ==================== Swagger ====================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ==================== Middleware order important ====================
// CORS trebuie înainte de auth ca browser-ul să nu se lovească de preflight
app.UseCors("UI");

// Rate limiting înainte de auth (ok)
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==================== Seed Admin + User + Produse ====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Creează tabelele dacă nu există
    context.Database.Migrate();

    // Cleanup expirate (poți să-l muți ulterior într-un background job, dar e ok dev)
    var expiredTokens = context.RefreshTokens.Where(t => t.Expires < DateTime.UtcNow);
    context.RefreshTokens.RemoveRange(expiredTokens);
    context.SaveChanges();

    // IMPORTANT: ai o mică neconcordanță în codul tău: verificai un email, inserai alt email.
    // Le aliniez ca să nu se tot insereze la fiecare pornire.
    if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
    {
        context.Users.Add(new User
        {
            Email = "admin@gmail.com",
            Password = PasswordHelper.HashPassword("adminA1."),
            Role = "Admin",
            IsActive = true,
            IsDeleted = false
        });
    }

    if (!context.Users.Any(u => u.Email == "user1@gmail.com"))
    {
        context.Users.Add(new User
        {
            Email = "user1@gmail.com",
            Password = PasswordHelper.HashPassword("userA11."),
            Role = "User",
            IsActive = true,
            IsDeleted = false
        });
    }

    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "Filtru aer", Manufacturer = "Bosch", Price = 120, Stock = 15 },
            new Product { Name = "Bujie", Manufacturer = "NGK", Price = 50, Stock = 30 },
            new Product { Name = "Ulei motor 5W30", Manufacturer = "Castrol", Price = 80, Stock = 20 },
            new Product { Name = "Filtru polen", Manufacturer = "Bosch", Price = 150, Stock = 13 }
        );
    }

    context.SaveChanges();
}

app.Run();