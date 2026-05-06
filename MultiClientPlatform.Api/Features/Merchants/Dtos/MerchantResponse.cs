namespace MultiClientPlatform.Api.Features.Merchants.Dtos;

public class MerchantResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
