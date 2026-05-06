using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Payments.Entities;
using MultiClientPlatform.Api.Features.Payments.Interfaces;

namespace MultiClientPlatform.Api.Features.Payments;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        return await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<Payment?> GetByIdAsync(int paymentId)
    {
        return await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task AddAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();
    }
}
