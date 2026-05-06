using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MultiClientPlatform.Api.Helpers;

// Responsible for generating and validating JWT tokens
public class JwtHelper
{
    private readonly IConfiguration _configuration;
    //IConfiguration is used to read settings from appsettings.json,appsettings.development.json,env variables
    //IConfiguration = “Give me values from appsettings.json”
    


    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int userId, string username, string role)
    {
        // Read JWT settings from appsettings.json
        string secretKey = _configuration["Jwt:Key"]!;
        string issuer = _configuration["Jwt:Issuer"]!;
        string audience = _configuration["Jwt:Audience"]!;
        int expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

        // The signing key — must match what is configured in Program.cs
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims are pieces of information embedded inside the token
        // ClaimTypes.NameIdentifier = userId (used for ownership checks)
        // ClaimTypes.Name = email, ClaimTypes.Role = role (used by [Authorize(Roles = "...")])
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, username));
        claims.Add(new Claim(ClaimTypes.Role, role));

        // Build the token
        JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        // Serialize the token to a string (the format: xxxxx.yyyyy.zzzzz)
        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }
}

