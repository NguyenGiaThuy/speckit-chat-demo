using IdentityService.Data.Entities;
using IdentityService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        (_tokenService, _, _) = TestTokenServiceFactory.Create();
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var token = _tokenService.GenerateAccessToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresCorrectly()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "exp@test.com", DisplayName = "Exp User" };
        (var shortTokenService, _, _) = TestTokenServiceFactory.Create(accessTtlMinutes: 1);

        var token = shortTokenService.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(1);
        Assert.True(jwt.ValidTo <= expectedExpiry.AddSeconds(5));
        Assert.True(jwt.ValidTo >= expectedExpiry.AddSeconds(-5));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsPlainTokenAndHash()
    {
        var (plain, hash) = _tokenService.GenerateRefreshToken();

        Assert.NotNull(plain);
        Assert.NotNull(hash);
        Assert.NotEmpty(plain);
        Assert.NotEmpty(hash);
        Assert.NotEqual(plain, hash);
    }

    [Fact]
    public void HashToken_IsDeterministic()
    {
        var token = "my-test-token";
        var hash1 = _tokenService.HashToken(token);
        var hash2 = _tokenService.HashToken(token);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashToken_DifferentInputsProduceDifferentHashes()
    {
        var hash1 = _tokenService.HashToken("token-a");
        var hash2 = _tokenService.HashToken("token-b");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GenerateRefreshToken_HashMatchesPlain()
    {
        var (plain, hash) = _tokenService.GenerateRefreshToken();
        var recomputed = _tokenService.HashToken(plain);

        Assert.Equal(hash, recomputed);
    }

    [Fact]
    public void AccessToken_CanBeValidatedWithPublicKey()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "valid@test.com", DisplayName = "Valid" };
        var token = _tokenService.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "test-issuer",
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _tokenService.GetPublicKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, validationParams, out var validatedToken);
        Assert.NotNull(principal);
        // JwtSecurityTokenHandler maps "sub" → ClaimTypes.NameIdentifier
        var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
        Assert.Equal(user.Id.ToString(), subClaim);
    }

    [Fact]
    public void GenerateRefreshTokenExpiry_IsInFuture()
    {
        var expiry = _tokenService.GetRefreshTokenExpiry();
        Assert.True(expiry > DateTime.UtcNow);
    }
}
