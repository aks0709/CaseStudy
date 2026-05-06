using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Cart.Entities;
using MultiClientPlatform.Api.Features.Cart.Interfaces;

namespace MultiClientPlatform.Api.Features.Cart;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;

    public CartRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task<CartItem?> GetItemAsync(int cartItemId)
    {
        return await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId);
    }

    public async Task<CartItem?> GetItemByProductAsync(int userId, int productId)
    {
        return await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task AddAsync(CartItem item)
    {
        _db.CartItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CartItem item)
    {
        _db.CartItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int userId)
    {
        List<CartItem> items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}
