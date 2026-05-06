namespace MultiClientPlatform.Api.Features.Orders.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    // Stored explicitly so merchants can query their own order items
    public int MerchantId { get; set; }

    // Snapshot price at time of order — not linked live to Product.Price
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
