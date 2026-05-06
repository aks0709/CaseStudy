using MultiClientPlatform.Api.Features.Orders.Entities;
using MultiClientPlatform.Api.Features.Orders.Interfaces;
using MultiClientPlatform.Api.Features.Payments.Dtos;
using MultiClientPlatform.Api.Features.Payments.Entities;
using MultiClientPlatform.Api.Features.Payments.Interfaces;

namespace MultiClientPlatform.Api.Features.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<(bool orderFound, bool authorized, bool alreadyPaid, InitiatePaymentResponse? response)> InitiateAsync(int userId, int orderId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, false, false, null);

        // Ownership check — order must belong to this customer
        if (order.UserId != userId)
            return (true, false, false, null);

        // Prevent duplicate payments
        Payment? existing = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (existing != null && existing.Status == "Completed")
            return (true, true, true, null);

        // Reuse pending payment if already initiated
        if (existing != null)
            return (true, true, false, MapToInitiateResponse(existing));

        // Generate a dummy payment URL simulating a gateway redirect
        string paymentUrl = $"https://dummy-gateway.example.com/pay?orderId={orderId}&amount={order.TotalAmount}&ref={Guid.NewGuid()}";

        var payment = new Payment
        {
            OrderId = orderId,
            Status = "Pending",
            PaymentUrl = paymentUrl
        };

        await _paymentRepository.AddAsync(payment);
        return (true, true, false, MapToInitiateResponse(payment));
    }

    public async Task<(bool found, bool authorized, PaymentStatusResponse? response)> CompleteAsync(int userId, int paymentId)
    {
        Payment? payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            return (false, false, null);

        Order? order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order == null || order.UserId != userId)
            return (true, false, null);

        // Mark payment and order as completed
        payment.Status = "Completed";
        payment.CompletedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);
        await _orderRepository.UpdateStatusAsync(order, "Paid");

        return (true, true, MapToStatusResponse(payment, order.Status));
    }

    public async Task<(bool found, bool authorized, PaymentStatusResponse? response)> GetStatusAsync(int userId, int orderId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return (false, false, null);

        if (order.UserId != userId)
            return (true, false, null);

        Payment? payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment == null)
            return (false, false, null);

        return (true, true, MapToStatusResponse(payment, order.Status));
    }

    private InitiatePaymentResponse MapToInitiateResponse(Payment payment)
    {
        return new InitiatePaymentResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Status = payment.Status,
            PaymentUrl = payment.PaymentUrl,
            InitiatedAt = payment.InitiatedAt
        };
    }

    private PaymentStatusResponse MapToStatusResponse(Payment payment, string orderStatus)
    {
        return new PaymentStatusResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            PaymentStatus = payment.Status,
            OrderStatus = orderStatus,
            InitiatedAt = payment.InitiatedAt,
            CompletedAt = payment.CompletedAt
        };
    }
}
