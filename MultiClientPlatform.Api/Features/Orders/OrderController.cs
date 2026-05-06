using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Merchants.Interfaces;
using MultiClientPlatform.Api.Features.Orders.Dtos;
using MultiClientPlatform.Api.Features.Orders.Interfaces;

namespace MultiClientPlatform.Api.Features.Orders;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMerchantRepository _merchantRepository;

    public OrderController(IOrderService orderService, IMerchantRepository merchantRepository)
    {
        _orderService = orderService;
        _merchantRepository = merchantRepository;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // POST api/order/checkout — Customer only
    [HttpPost("checkout")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderResponse>> Checkout()
    {
        var (hasItems, order) = await _orderService.CheckoutAsync(GetUserId());

        if (!hasItems)
            return BadRequest("Cart is empty. Add products before checking out.");

        return Ok(order);
    }

    // GET api/order — Customer views their own orders
    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
    {
        List<OrderResponse> orders = await _orderService.GetMyOrdersAsync(GetUserId());
        return Ok(orders);
    }

    // GET api/order/{id} — Customer views a single order (ownership enforced)
    [HttpGet("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        var (found, authorized, order) = await _orderService.GetOrderByIdAsync(GetUserId(), id);

        if (!found)
            return NotFound("Order not found.");

        if (!authorized)
            return Forbid();

        return Ok(order);
    }

    // GET api/order/merchant-items — Merchant views only their order items
    [HttpGet("merchant-items")]
    [Authorize(Roles = "Merchant")]
    public async Task<ActionResult<List<OrderItemResponse>>> GetMerchantOrderItems()
    {
        var merchant = await _merchantRepository.GetByUserIdAsync(GetUserId());
        if (merchant == null)
            return NotFound("Merchant profile not found.");

        List<OrderItemResponse> items = await _orderService.GetMyOrderItemsAsync(merchant.Id);
        return Ok(items);
    }
}
