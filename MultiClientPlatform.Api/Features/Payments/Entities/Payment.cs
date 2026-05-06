namespace MultiClientPlatform.Api.Features.Payments.Entities;

public class Payment
{
    public int Id { get; set; }

    // 1-to-1 with Order
    public int OrderId { get; set; }

    // Pending | Completed | Failed
    public string Status { get; set; } = "Pending";

    // Dummy redirect URL simulating a payment gateway
    public string PaymentUrl { get; set; } = string.Empty;

    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
