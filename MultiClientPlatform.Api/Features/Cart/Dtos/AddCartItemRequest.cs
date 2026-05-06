namespace MultiClientPlatform.Api.Features.Cart.Dtos;

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
