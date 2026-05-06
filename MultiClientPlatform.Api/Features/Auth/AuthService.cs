using MultiClientPlatform.Api.Features.Auth.Dtos;
using MultiClientPlatform.Api.Features.Auth.Entities;
using MultiClientPlatform.Api.Features.Auth.Interfaces;
using MultiClientPlatform.Api.Helpers;

namespace MultiClientPlatform.Api.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IAuthRepository authRepository, JwtHelper jwtHelper)
    {
        _authRepository = authRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Reject if email already exists
        bool emailExists = await _authRepository.EmailExistsAsync(request.Email);
        if (emailExists)
            return null;

        // Validate role
        if (request.Role != "Customer" && request.Role != "Merchant")
            return null;

        //Building a new user object to be added to the database
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        //await stops functions excution so token is only generated when user is added to the database
        await _authRepository.AddUserAsync(user);

        string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Find user by email
        User? user = await _authRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return null;

        // Verify password against stored hash
        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            return null;

        string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
