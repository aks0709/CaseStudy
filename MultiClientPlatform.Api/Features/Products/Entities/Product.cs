namespace MultiClientPlatform.Api.Features.Products.Entities;

public class Product
{
    public int Id { get; set; }

    // Ownership — links to the Merchant who owns this product
    public int MerchantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
