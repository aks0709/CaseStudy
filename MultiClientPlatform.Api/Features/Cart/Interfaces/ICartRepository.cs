using MultiClientPlatform.Api.Features.Cart.Entities;

namespace MultiClientPlatform.Api.Features.Cart.Interfaces;

public interface ICartRepository
{
    Task<List<CartItem>> GetByUserIdAsync(int userId);
    Task<CartItem?> GetItemAsync(int cartItemId);
    Task<CartItem?> GetItemByProductAsync(int userId, int productId);
    Task AddAsync(CartItem item);
    Task UpdateAsync(CartItem item);
    Task DeleteAsync(CartItem item);
    Task ClearCartAsync(int userId);
}
