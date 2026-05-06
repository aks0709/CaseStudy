namespace MultiClientPlatform.Api.Features.Cart.Dtos;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();

    // Sum of all line totals
    public decimal GrandTotal { get; set; }
}
