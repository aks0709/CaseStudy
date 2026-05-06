using MultiClientPlatform.Api.Features.Cart.Interfaces;
using MultiClientPlatform.Api.Features.Merchants.Interfaces;
using MultiClientPlatform.Api.Features.Orders.Dtos;
using MultiClientPlatform.Api.Features.Orders.Entities;
using MultiClientPlatform.Api.Features.Orders.Interfaces;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMerchantRepository _merchantRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IMerchantRepository merchantRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _merchantRepository = merchantRepository;
    }

    public async Task<(bool hasItems, OrderResponse? order)> CheckoutAsync(int userId)
    {
        var cartItems = await _cartRepository.GetByUserIdAsync(userId);
        if (cartItems.Count == 0)
            return (false, null);

        var order = new Order { UserId = userId };

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null) continue;

            decimal lineTotal = product.Price * cartItem.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                MerchantId = product.MerchantId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal
            });

            order.TotalAmount += lineTotal;
        }

        await _orderRepository.AddAsync(order);

        // Clear cart after successful order placement
        await _cartRepository.ClearCartAsync(userId);

        return (true, MapToResponse(order));
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<(bool found, bool authorized, OrderResponse? order)> GetOrderByIdAsync(int userId, int orderId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, false, null);

        // Ownership check — order must belong to this customer
        if (order.UserId != userId)
            return (true, false, null);

        return (true, true, MapToResponse(order));
    }

    public async Task<List<OrderItemResponse>> GetMyOrderItemsAsync(int merchantId)
    {
        var items = await _orderRepository.GetItemsByMerchantIdAsync(merchantId);
        return items.Select(MapToItemResponse).ToList();
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            PlacedAt = order.PlacedAt,
            Items = order.Items.Select(MapToItemResponse).ToList()
        };
    }

    private OrderItemResponse MapToItemResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            MerchantId = item.MerchantId,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        };
    }
}
