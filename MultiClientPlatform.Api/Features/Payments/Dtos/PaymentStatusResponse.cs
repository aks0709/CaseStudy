namespace MultiClientPlatform.Api.Features.Payments.Dtos;

public class PaymentStatusResponse
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
