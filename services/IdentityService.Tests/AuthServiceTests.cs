using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var (tokenService, _, _) = TestTokenServiceFactory.Create();
        _authService = new AuthService(_db, tokenService);
    }

    public void Dispose() => _db.Dispose();

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns201Data()
    {
        var req = new RegisterRequest
        {
            Email = "alice@example.com",
            Password = "Password1",
            DisplayName = "Alice"
        };

        var (success, error, result) = await _authService.RegisterAsync(req);

        Assert.True(success);
        Assert.Null(error);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == "alice@example.com");
        Assert.NotNull(user);
        Assert.Equal("Alice", user.DisplayName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsEmailTakenError()
    {
        var req = new RegisterRequest { Email = "dup@example.com", Password = "Password1", DisplayName = "Dup" };
        await _authService.RegisterAsync(req);

        var (success, error, _) = await _authService.RegisterAsync(req);

        Assert.False(success);
        Assert.Equal("email_taken", error);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationError()
    {
        var req = new RegisterRequest { Email = "weak@example.com", Password = "short", DisplayName = "Weak" };

        var (success, error, _) = await _authService.RegisterAsync(req);

        Assert.False(success);
        Assert.Equal("validation_error", error);
    }

    [Fact]
    public async Task Register_PasswordWithNoUppercase_ReturnsValidationError()
    {
        var req = new RegisterRequest { Email = "noupper@example.com", Password = "password1", DisplayName = "NoUpper" };

        var (success, error, _) = await _authService.RegisterAsync(req);

        Assert.False(success);
        Assert.Equal("validation_error", error);
    }

    [Fact]
    public async Task Register_PasswordWithNoNumber_ReturnsValidationError()
    {
        var req = new RegisterRequest { Email = "nonum@example.com", Password = "PasswordABC", DisplayName = "NoNum" };

        var (success, error, _) = await _authService.RegisterAsync(req);

        Assert.False(success);
        Assert.Equal("validation_error", error);
    }

    [Fact]
    public async Task Register_StoresPasswordHash_NotPlaintext()
    {
        var req = new RegisterRequest { Email = "hash@example.com", Password = "Password1", DisplayName = "Hash" };
        await _authService.RegisterAsync(req);

        var user = await _db.Users.FirstAsync(u => u.Email == "hash@example.com");
        Assert.NotEqual("Password1", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password1", user.PasswordHash));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenPair()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "login@example.com", Password = "Password1", DisplayName = "Login User"
        });

        var (success, _, result) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "login@example.com", Password = "Password1"
        });

        Assert.True(success);
        var tokenResponse = Assert.IsType<TokenResponse>(result);
        Assert.NotEmpty(tokenResponse.AccessToken);
        Assert.NotEmpty(tokenResponse.RefreshToken);
        Assert.Equal(900, tokenResponse.ExpiresIn); // 15 min * 60
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalidCredentials()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "wrongpw@example.com", Password = "Password1", DisplayName = "Wrong"
        });

        var (success, error, _) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "wrongpw@example.com", Password = "WrongPass9"
        });

        Assert.False(success);
        Assert.Equal("invalid_credentials", error);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsInvalidCredentials()
    {
        var (success, error, _) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "noone@example.com", Password = "Password1"
        });

        Assert.False(success);
        Assert.Equal("invalid_credentials", error);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokenPair()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "refresh@example.com", Password = "Password1", DisplayName = "Refresh"
        });
        var (_, _, loginResult) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "refresh@example.com", Password = "Password1"
        });
        var originalTokens = Assert.IsType<TokenResponse>(loginResult);

        var (success, _, refreshResult) = await _authService.RefreshAsync(originalTokens.RefreshToken);

        Assert.True(success);
        var newTokens = Assert.IsType<TokenResponse>(refreshResult);
        Assert.NotEmpty(newTokens.AccessToken);
        Assert.NotEmpty(newTokens.RefreshToken);
        Assert.NotEqual(originalTokens.RefreshToken, newTokens.RefreshToken); // rotated
    }

    [Fact]
    public async Task Refresh_UsedTokenIsRevoked_ReturnsFalse()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "rotate@example.com", Password = "Password1", DisplayName = "Rotate"
        });
        var (_, _, loginResult) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "rotate@example.com", Password = "Password1"
        });
        var tokens = Assert.IsType<TokenResponse>(loginResult);

        await _authService.RefreshAsync(tokens.RefreshToken); // use once

        var (success, error, _) = await _authService.RefreshAsync(tokens.RefreshToken); // reuse

        Assert.False(success);
        Assert.Equal("invalid_token", error);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsFalse()
    {
        var (success, error, _) = await _authService.RefreshAsync("not-a-real-token");

        Assert.False(success);
        Assert.Equal("invalid_token", error);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_ValidToken_RevokesRefreshToken()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "logout@example.com", Password = "Password1", DisplayName = "Logout"
        });
        var (_, _, loginResult) = await _authService.LoginAsync(new LoginRequest
        {
            Email = "logout@example.com", Password = "Password1"
        });
        var tokens = Assert.IsType<TokenResponse>(loginResult);

        var loggedOut = await _authService.LogoutAsync(tokens.RefreshToken);
        Assert.True(loggedOut);

        // Subsequent refresh should fail
        var (success, error, _) = await _authService.RefreshAsync(tokens.RefreshToken);
        Assert.False(success);
        Assert.Equal("invalid_token", error);
    }

    [Fact]
    public async Task Logout_InvalidToken_ReturnsFalse()
    {
        var result = await _authService.LogoutAsync("fake-token");
        Assert.False(result);
    }
}
