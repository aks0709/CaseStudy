using MultiClientPlatform.Api.Features.Auth.Dtos;

namespace MultiClientPlatform.Api.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
