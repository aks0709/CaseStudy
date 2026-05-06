using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Products.Entities;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Products;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetByMerchantIdAsync(int merchantId)
    {
        return await _db.Products.Where(p => p.MerchantId == merchantId).ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }
}
