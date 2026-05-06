using MultiClientPlatform.Api.Features.Payments.Dtos;

namespace MultiClientPlatform.Api.Features.Payments.Interfaces;

public interface IPaymentService
{
    // Initiate payment for an order — generates dummy URL
    Task<(bool orderFound, bool authorized, bool alreadyPaid, InitiatePaymentResponse? response)> InitiateAsync(int userId, int orderId);

    // Simulate payment completion — marks payment and order as Paid
    Task<(bool found, bool authorized, PaymentStatusResponse? response)> CompleteAsync(int userId, int paymentId);

    // Get payment status for an order
    Task<(bool found, bool authorized, PaymentStatusResponse? response)> GetStatusAsync(int userId, int orderId);
}
