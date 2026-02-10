using AutoPartsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsShop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // <-- adăugat
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<CartItem>()
        .HasIndex(ci => new { ci.UserId, ci.ProductId })
        .IsUnique();
}
}
