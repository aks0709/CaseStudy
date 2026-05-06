using MultiClientPlatform.Api.Features.Payments.Entities;

namespace MultiClientPlatform.Api.Features.Payments.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByIdAsync(int paymentId);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
