using Mechanics.Infra.Data.Seeds;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Mechanics.Tests.Behavior.Hooks;

public class TestTokenGenerator(RSA rsa)
{
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public string GenerateAccessTokenByRoleName(string roleName)
    {
        var roleId = RoleSeeds.GetSeeds().First(r => r.Name == roleName).Id;
        var user = UserSeeds.GetSeeds().First(u => u.RoleId == roleId);

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("customerId", (user.CustomerId ?? Guid.Empty).ToString()),
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
