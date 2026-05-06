using MultiClientPlatform.Api.Features.Orders.Dtos;

namespace MultiClientPlatform.Api.Features.Orders.Interfaces;

public interface IOrderService
{
    // Customer: convert cart to order and clear cart
    Task<(bool hasItems, OrderResponse? order)> CheckoutAsync(int userId);

    // Customer: view all their orders
    Task<List<OrderResponse>> GetMyOrdersAsync(int userId);

    // Customer: view a single order (ownership enforced)
    Task<(bool found, bool authorized, OrderResponse? order)> GetOrderByIdAsync(int userId, int orderId);

    // Merchant: view only their order items across all orders
    Task<List<OrderItemResponse>> GetMyOrderItemsAsync(int merchantId);
}
