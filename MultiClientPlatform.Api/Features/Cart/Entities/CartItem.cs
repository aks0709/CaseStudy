namespace MultiClientPlatform.Api.Features.Cart.Entities;

public class CartItem
{
    public int Id { get; set; }

    // Ownership — cart belongs to this customer
    public int UserId { get; set; }

    // The product added to cart
    public int ProductId { get; set; }

    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
