using MultiClientPlatform.Api.Features.Merchants.Interfaces;
using MultiClientPlatform.Api.Features.Products.Dtos;
using MultiClientPlatform.Api.Features.Products.Entities;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMerchantRepository _merchantRepository;

    public ProductService(IProductRepository productRepository, IMerchantRepository merchantRepository)
    {
        _productRepository = productRepository;
        _merchantRepository = merchantRepository;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        List<Product> products = await _productRepository.GetAllAsync();
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        Product? product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        return MapToResponse(product);
    }

    public async Task<List<ProductResponse>> GetByMerchantIdAsync(int merchantId)
    {
        List<Product> products = await _productRepository.GetByMerchantIdAsync(merchantId);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse?> CreateAsync(int userId, CreateProductRequest request)
    {
        // Merchant profile must exist before creating a product
        var merchant = await _merchantRepository.GetByUserIdAsync(userId);
        if (merchant == null)
            return null;

        var product = new Product
        {
            MerchantId = merchant.Id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        await _productRepository.AddAsync(product);
        return MapToResponse(product);
    }

    public async Task<(bool found, bool authorized, ProductResponse? product)> UpdateAsync(int userId, int productId, UpdateProductRequest request)
    {
        Product? product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return (false, false, null);

        // Verify the product belongs to the merchant of this user
        var merchant = await _merchantRepository.GetByUserIdAsync(userId);
        if (merchant == null || product.MerchantId != merchant.Id)
            return (true, false, null);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        await _productRepository.UpdateAsync(product);
        return (true, true, MapToResponse(product));
    }

    // Maps Product entity to ProductResponse DTO
    private ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            MerchantId = product.MerchantId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };
    }
}
