namespace MultiClientPlatform.Api.Features.Auth.Dtos;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Must be "Customer" or "Merchant"
    public string Role { get; set; } = string.Empty;
}
