namespace MultiClientPlatform.Api.Features.Merchants.Entities;

public class Merchant
{
    public int Id { get; set; }

    // Ownership — links to the User who created this profile
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
