using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Auth.Dtos;
using MultiClientPlatform.Api.Features.Auth.Interfaces;

namespace MultiClientPlatform.Api.Features.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        AuthResponse? response = await _authService.RegisterAsync(request);

        if (response == null)
            return BadRequest("Email already exists or role is invalid. Use 'Customer' or 'Merchant'.");

        return Ok(response);
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        AuthResponse? response = await _authService.LoginAsync(request);

        if (response == null)
            return Unauthorized("Invalid email or password.");

        return Ok(response);
    }
}
