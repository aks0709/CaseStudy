using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Orders.Entities;
using MultiClientPlatform.Api.Features.Orders.Interfaces;

namespace MultiClientPlatform.Api.Features.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        // Include items so the response is fully populated
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<OrderItem>> GetItemsByMerchantIdAsync(int merchantId)
    {
        return await _db.OrderItems
            .Where(i => i.MerchantId == merchantId)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(Order order, string status)
    {
        order.Status = status;
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }
}
