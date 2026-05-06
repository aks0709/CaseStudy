using MultiClientPlatform.Api.Features.Orders.Entities;

namespace MultiClientPlatform.Api.Features.Orders.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdAsync(int orderId);
    Task UpdateStatusAsync(Order order, string status);

    // Returns only order items belonging to a specific merchant
    Task<List<OrderItem>> GetItemsByMerchantIdAsync(int merchantId);
}
