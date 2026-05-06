using MultiClientPlatform.Api.Features.Cart.Dtos;

namespace MultiClientPlatform.Api.Features.Cart.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);
    Task<(bool productExists, CartItemResponse? item)> AddItemAsync(int userId, AddCartItemRequest request);
    Task<(bool found, bool authorized, CartItemResponse? item)> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task<(bool found, bool authorized)> RemoveItemAsync(int userId, int cartItemId);
}
