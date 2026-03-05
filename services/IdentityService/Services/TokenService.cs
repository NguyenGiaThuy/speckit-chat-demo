using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityService.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public class TokenService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;
    private readonly string _issuer;
    private readonly int _accessTtlMinutes;
    private readonly int _refreshTtlDays;

    public TokenService(IConfiguration config)
    {
        var privateKeyPem = config["JWT_PRIVATE_KEY"]
            ?? throw new InvalidOperationException("JWT_PRIVATE_KEY is not configured");
        var publicKeyPem = config["JWT_PUBLIC_KEY"]
            ?? throw new InvalidOperationException("JWT_PUBLIC_KEY is not configured");

        // Docker/shell env vars encode newlines as literal \n — restore them for PEM parsing
        privateKeyPem = privateKeyPem.Replace("\\n", "\n");
        publicKeyPem = publicKeyPem.Replace("\\n", "\n");

        var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKeyPem);
        _privateKey = new RsaSecurityKey(rsaPrivate);

        var rsaPublic = RSA.Create();
        rsaPublic.ImportFromPem(publicKeyPem);
        _publicKey = new RsaSecurityKey(rsaPublic);

        _issuer = config["JWT_ISSUER"] ?? "speckit-chat";
        _accessTtlMinutes = int.TryParse(config["JWT_ACCESS_TTL_MINUTES"], out var ttl) ? ttl : 15;
        _refreshTtlDays = int.TryParse(config["JWT_REFRESH_TTL_DAYS"], out var rttl) ? rttl : 7;
    }

    public string GenerateAccessToken(User user)
    {
        var credentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("display_name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_accessTtlMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string plainToken, string tokenHash) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plainToken = Convert.ToBase64String(bytes);
        var tokenHash = HashToken(plainToken);
        return (plainToken, tokenHash);
    }

    public DateTime GetRefreshTokenExpiry() =>
        DateTime.UtcNow.AddDays(_refreshTtlDays);

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public RsaSecurityKey GetPublicKey() => _publicKey;
    public string GetIssuer() => _issuer;
    public int GetAccessTtlMinutes() => _accessTtlMinutes;
}
