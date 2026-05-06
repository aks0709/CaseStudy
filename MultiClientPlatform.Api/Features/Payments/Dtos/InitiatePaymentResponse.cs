namespace MultiClientPlatform.Api.Features.Payments.Dtos;

public class InitiatePaymentResponse
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;

    // Dummy URL the customer would be redirected to
    public string PaymentUrl { get; set; } = string.Empty;

    public DateTime InitiatedAt { get; set; }
}
