namespace MultiClientPlatform.Api.Features.Orders.Entities;

public class Order
{
    public int Id { get; set; }

    // Customer who placed the order
    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    // Pending → Paid (updated when payment is confirmed)
    public string Status { get; set; } = "Pending";

    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    // Navigation — order line items
    public List<OrderItem> Items { get; set; } = new();
}
