namespace MultiClientPlatform.Api.Features.Orders.Dtos;

public class OrderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PlacedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}
