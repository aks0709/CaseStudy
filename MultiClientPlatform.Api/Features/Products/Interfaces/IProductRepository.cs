using MultiClientPlatform.Api.Features.Products.Entities;

namespace MultiClientPlatform.Api.Features.Products.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetByMerchantIdAsync(int merchantId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
}
