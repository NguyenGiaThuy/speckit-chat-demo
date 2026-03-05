using System.Security.Cryptography;
using IdentityService.Services;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Tests;

/// <summary>
/// Provides a TokenService configured with a freshly generated RSA key pair for use in unit tests.
/// </summary>
public static class TestTokenServiceFactory
{
    public static (TokenService tokenService, string privateKeyPem, string publicKeyPem) Create(
        int accessTtlMinutes = 15,
        int refreshTtlDays = 7,
        string issuer = "test-issuer")
    {
        using var rsa = RSA.Create(2048);
        var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
        var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_PRIVATE_KEY"] = privateKeyPem,
                ["JWT_PUBLIC_KEY"] = publicKeyPem,
                ["JWT_ISSUER"] = issuer,
                ["JWT_ACCESS_TTL_MINUTES"] = accessTtlMinutes.ToString(),
                ["JWT_REFRESH_TTL_DAYS"] = refreshTtlDays.ToString()
            })
            .Build();

        return (new TokenService(config), privateKeyPem, publicKeyPem);
    }
}
