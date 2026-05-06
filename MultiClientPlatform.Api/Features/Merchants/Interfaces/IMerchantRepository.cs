using MultiClientPlatform.Api.Features.Merchants.Entities;

namespace MultiClientPlatform.Api.Features.Merchants.Interfaces;

public interface IMerchantRepository
{
    Task<bool> ExistsByUserIdAsync(int userId);
    Task<Merchant?> GetByUserIdAsync(int userId);
    Task<Merchant?> GetByIdAsync(int id);
    Task<List<Merchant>> GetAllAsync();
    Task AddAsync(Merchant merchant);
}
