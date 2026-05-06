namespace MultiClientPlatform.Api.Features.Auth.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Hashed password — never store plain text
    public string PasswordHash { get; set; } = string.Empty;

    // "Customer" or "Merchant"
    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
