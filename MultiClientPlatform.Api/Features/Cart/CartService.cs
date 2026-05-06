using MultiClientPlatform.Api.Features.Cart.Dtos;
using MultiClientPlatform.Api.Features.Cart.Entities;
using MultiClientPlatform.Api.Features.Cart.Interfaces;
using MultiClientPlatform.Api.Features.Products.Entities;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Cart;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        List<CartItem> items = await _cartRepository.GetByUserIdAsync(userId);

        List<CartItemResponse> responseItems = new List<CartItemResponse>();

        foreach (CartItem item in items)
        {
            Product? product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null) continue;

            responseItems.Add(MapToItemResponse(item, product));
        }

        return new CartResponse
        {
            Items = responseItems,
            GrandTotal = responseItems.Sum(i => i.LineTotal)
        };
    }

    public async Task<(bool productExists, CartItemResponse? item)> AddItemAsync(int userId, AddCartItemRequest request)
    {
        Product? product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            return (false, null);

        // If product already in cart, increase quantity instead of adding duplicate
        CartItem? existing = await _cartRepository.GetItemByProductAsync(userId, request.ProductId);
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
            await _cartRepository.UpdateAsync(existing);
            return (true, MapToItemResponse(existing, product));
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        await _cartRepository.AddAsync(cartItem);
        return (true, MapToItemResponse(cartItem, product));
    }

    public async Task<(bool found, bool authorized, CartItemResponse? item)> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        CartItem? cartItem = await _cartRepository.GetItemAsync(cartItemId);
        if (cartItem == null)
            return (false, false, null);

        // Ownership check — cart item must belong to this user
        if (cartItem.UserId != userId)
            return (true, false, null);

        cartItem.Quantity = request.Quantity;
        await _cartRepository.UpdateAsync(cartItem);

        Product? product = await _productRepository.GetByIdAsync(cartItem.ProductId);
        return (true, true, MapToItemResponse(cartItem, product!));
    }

    public async Task<(bool found, bool authorized)> RemoveItemAsync(int userId, int cartItemId)
    {
        CartItem? cartItem = await _cartRepository.GetItemAsync(cartItemId);
        if (cartItem == null)
            return (false, false);

        // Ownership check — cart item must belong to this user
        if (cartItem.UserId != userId)
            return (true, false);

        await _cartRepository.DeleteAsync(cartItem);
        return (true, true);
    }

    // Maps CartItem + Product into a CartItemResponse with computed line total
    private CartItemResponse MapToItemResponse(CartItem item, Product product)
    {
        return new CartItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = item.Quantity,
            LineTotal = product.Price * item.Quantity
        };
    }
}
