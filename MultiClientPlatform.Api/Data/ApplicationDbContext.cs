using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Features.Auth.Entities;
using MultiClientPlatform.Api.Features.Cart.Entities;
using MultiClientPlatform.Api.Features.Merchants.Entities;
using MultiClientPlatform.Api.Features.Orders.Entities;
using MultiClientPlatform.Api.Features.Payments.Entities;
using MultiClientPlatform.Api.Features.Products.Entities;

namespace MultiClientPlatform.Api.Data;

// Central EF Core DbContext — DbSets will be added per feature
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
}
