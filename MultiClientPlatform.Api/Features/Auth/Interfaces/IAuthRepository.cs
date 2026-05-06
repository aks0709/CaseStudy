using MultiClientPlatform.Api.Features.Auth.Entities;

namespace MultiClientPlatform.Api.Features.Auth.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task AddUserAsync(User user);
}
