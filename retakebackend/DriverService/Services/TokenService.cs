using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DriverService.Models;
using Microsoft.IdentityModel.Tokens;

namespace DriverService.Services;

public class TokenService(IConfiguration configuration)
{
    public string GenerateDriverToken(Driver driver)
    {
        var key = configuration["Jwt:Key"] ?? "super-secret-dev-key-change-me-32bytes";
        var issuer = configuration["Jwt:Issuer"] ?? "DriverService";
        var audience = configuration["Jwt:Audience"] ?? "TaxiClients";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, driver.Id.ToString()),
            new Claim("name", driver.Name),
            new Claim("status", driver.Status.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
