using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Mechanics.Tests.Integration.Helpers;

public class TestTokenGenerator(RSA rsa)
{
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public string GenerateAccessTokenByRoleName(string roleName)
    {
        var claims = new List<Claim>
        {
            new("sub", "3ef30b85-9e4e-4066-b460-0a722216c51b"),
            new("customerId", (Guid.Empty).ToString()),
            new("role", roleName),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = "fiap-mechanics",
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256),
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }
}
