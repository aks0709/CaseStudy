using Microsoft.EntityFrameworkCore;
using MultiClientPlatform.Api.Data;
using MultiClientPlatform.Api.Features.Auth.Entities;
using MultiClientPlatform.Api.Features.Auth.Interfaces;

namespace MultiClientPlatform.Api.Features.Auth;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _db;

    public AuthRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    //_db.Users is the User Table in the database
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}
