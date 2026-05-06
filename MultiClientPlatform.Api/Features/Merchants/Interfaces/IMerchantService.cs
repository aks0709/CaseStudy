using MultiClientPlatform.Api.Features.Merchants.Dtos;

namespace MultiClientPlatform.Api.Features.Merchants.Interfaces;

public interface IMerchantService
{
    Task<MerchantResponse?> CreateProfileAsync(int userId, CreateMerchantRequest request);
    Task<MerchantResponse?> GetMyProfileAsync(int userId);
    Task<List<MerchantResponse>> GetAllAsync();
    Task<MerchantResponse?> GetByIdAsync(int id);
}
