using MultiClientPlatform.Api.Features.Products.Dtos;

namespace MultiClientPlatform.Api.Features.Products.Interfaces;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<List<ProductResponse>> GetByMerchantIdAsync(int merchantId);
    Task<ProductResponse?> CreateAsync(int userId, CreateProductRequest request);

    // Returns null if product not found, false if ownership check fails
    Task<(bool found, bool authorized, ProductResponse? product)> UpdateAsync(int userId, int productId, UpdateProductRequest request);
}
