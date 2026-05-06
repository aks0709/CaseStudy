using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Cart.Dtos;
using MultiClientPlatform.Api.Features.Cart.Interfaces;

namespace MultiClientPlatform.Api.Features.Cart;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // Extracts UserId from the JWT token,to make sure only particular user's cart is accessed/modified
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET api/cart
    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart()
    {
        CartResponse cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(cart);
    }

    // POST api/cart
    [HttpPost]
    public async Task<ActionResult<CartItemResponse>> AddItem(AddCartItemRequest request)
    {
        var (productExists, item) = await _cartService.AddItemAsync(GetUserId(), request);

        if (!productExists)
            return NotFound("Product not found.");

        return Ok(item);
    }

    // PUT api/cart/{cartItemId}
    [HttpPut("{cartItemId}")]
    public async Task<ActionResult<CartItemResponse>> UpdateItem(int cartItemId, UpdateCartItemRequest request)
    {
        var (found, authorized, item) = await _cartService.UpdateItemAsync(GetUserId(), cartItemId, request);

        if (!found)
            return NotFound("Cart item not found.");

        if (!authorized)
            return Forbid();

        return Ok(item);
    }

    // DELETE api/cart/{cartItemId}
    [HttpDelete("{cartItemId}")]
    public async Task<ActionResult> RemoveItem(int cartItemId)
    {
        var (found, authorized) = await _cartService.RemoveItemAsync(GetUserId(), cartItemId);

        if (!found)
            return NotFound("Cart item not found.");

        if (!authorized)
            return Forbid();

        return NoContent();
    }
}
