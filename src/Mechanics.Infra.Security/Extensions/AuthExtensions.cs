using Mechanics.Infra.Security.Models;
using Mechanics.Infra.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Mechanics.Infra.Security.Extensions;

public static class AuthExtensions
{
    private const string JwtTokenIssuer = "fiap-mechanics";

    /// <summary>
    ///     Configura autenticação por JWT.
    /// </summary>
    /// <remarks>A chave do token sempre é validada em modo de Release.</remarks>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IWebHostEnvironment environment)
    {
        return !environment.IsDevelopment()
            ? services.AddValidatedAuthentication()
            : services.AddAuthenticationWithoutValidation();
    }

    /// <summary>
    ///     Configura autenticação sem validação de token.
    /// </summary>
    /// <remarks>Utilize somente em serviços onde o API Gateway já faz a validação do token JWT.</remarks>
    private static IServiceCollection AddAuthenticationWithoutValidation(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = JwtTokenIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    SignatureValidator = (token, _) => new JsonWebToken(token),
                };
            });

        services.AddAuthorizationPolicies()
            .AddCurrentUserService();

        return services;
    }

    /// <summary>
    ///     Configura autenticação que valida o token JWT através da chave pública.
    /// </summary>
    private static IServiceCollection AddValidatedAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtTokenIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = LoadSecurityKey(),
                };
            });

        services.AddAuthorizationPolicies()
            .AddCurrentUserService();

        return services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.EmployeesOnly, policy =>
                policy.RequireRole(RoleNames.Administrator, RoleNames.Attendant, RoleNames.Mechanic))
            .AddPolicy(PolicyNames.CustomersOnly, policy =>
                policy.RequireRole(RoleNames.CustomerAdmin, RoleNames.CustomerUser))
            .AddPolicy(PolicyNames.AllAuthenticated, policy => policy.RequireAuthenticatedUser());

        return services;
    }

    private static void AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ClaimsPrincipal>(provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

    private static RsaSecurityKey LoadSecurityKey()
    {
        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "keys", "jwt-public.pem");
        if (!File.Exists(publicKeyPath))
            throw new InvalidOperationException("JWT Public Key is not available.");

        var publicKey = File.ReadAllText(publicKeyPath);

        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);

        return new RsaSecurityKey(rsa);
    }
}
