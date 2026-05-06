using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Merchants.Entities;
using MultiClientPlatform.Api.Features.Merchants.Interfaces;

namespace MultiClientPlatform.Api.Features.Merchants;

public class MerchantRepository : IMerchantRepository
{
    private readonly ApplicationDbContext _db;

    public MerchantRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
    {
        return await _db.Merchants.AnyAsync(m => m.UserId == userId);
    }

    public async Task<Merchant?> GetByUserIdAsync(int userId)
    {
        return await _db.Merchants.FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<Merchant?> GetByIdAsync(int id)
    {
        return await _db.Merchants.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Merchant>> GetAllAsync()
    {
        return await _db.Merchants.ToListAsync();
    }

    public async Task AddAsync(Merchant merchant)
    {
        _db.Merchants.Add(merchant);
        await _db.SaveChangesAsync();
    }
}
