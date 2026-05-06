using MultiClientPlatform.Api.Features.Merchants.Dtos;
using MultiClientPlatform.Api.Features.Merchants.Entities;
using MultiClientPlatform.Api.Features.Merchants.Interfaces;

namespace MultiClientPlatform.Api.Features.Merchants;

public class MerchantService : IMerchantService
{
    private readonly IMerchantRepository _merchantRepository;

    public MerchantService(IMerchantRepository merchantRepository)
    {
        _merchantRepository = merchantRepository;
    }

    public async Task<MerchantResponse?> CreateProfileAsync(int userId, CreateMerchantRequest request)
    {
        // One merchant profile per user
        bool alreadyExists = await _merchantRepository.ExistsByUserIdAsync(userId);
        if (alreadyExists)
            return null;

        var merchant = new Merchant
        {
            UserId = userId,
            BusinessName = request.BusinessName,
            Description = request.Description
        };

        await _merchantRepository.AddAsync(merchant);

        return MapToResponse(merchant);
    }

    public async Task<MerchantResponse?> GetMyProfileAsync(int userId)
    {
        Merchant? merchant = await _merchantRepository.GetByUserIdAsync(userId);
        if (merchant == null)
            return null;

        return MapToResponse(merchant);
    }

    public async Task<List<MerchantResponse>> GetAllAsync()
    {
        List<Merchant> merchants = await _merchantRepository.GetAllAsync();
        return merchants.Select(MapToResponse).ToList();
    }

    public async Task<MerchantResponse?> GetByIdAsync(int id)
    {
        Merchant? merchant = await _merchantRepository.GetByIdAsync(id);
        if (merchant == null)
            return null;

        return MapToResponse(merchant);
    }

    // Maps Merchant entity to MerchantResponse DTO
    private MerchantResponse MapToResponse(Merchant merchant)
    {
        return new MerchantResponse
        {
            Id = merchant.Id,
            UserId = merchant.UserId,
            BusinessName = merchant.BusinessName,
            Description = merchant.Description,
            CreatedAt = merchant.CreatedAt
        };
    }
}
