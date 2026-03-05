using IdentityService.Data;
using IdentityService.Data.Entities;
using IdentityService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthService(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<(bool success, string? error, object? result)> RegisterAsync(RegisterRequest req)
    {
        if (!IsValidPassword(req.Password))
            return (false, "validation_error", new { error = "validation_error", details = "Password must be ≥8 chars with at least one uppercase letter and one number." });

        var exists = await _db.Users.AnyAsync(u => u.Email == req.Email.ToLowerInvariant());
        if (exists)
            return (false, "email_taken", new { error = "email_taken" });

        var user = new User
        {
            Email = req.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
            DisplayName = req.DisplayName,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, null, new { id = user.Id, display_name = user.DisplayName });
    }

    public async Task<(bool success, string? error, object? result)> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant());
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return (false, "invalid_credentials", new { error = "invalid_credentials" });

        var accessToken = _tokenService.GenerateAccessToken(user);
        var (plainRefresh, refreshHash) = _tokenService.GenerateRefreshToken();

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = _tokenService.GetRefreshTokenExpiry()
        };
        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return (true, null, new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = plainRefresh,
            ExpiresIn = _tokenService.GetAccessTtlMinutes() * 60
        });
    }

    public async Task<(bool success, string? error, object? result)> RefreshAsync(string plainRefreshToken)
    {
        var tokenHash = _tokenService.HashToken(plainRefreshToken);

        var rt = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        if (rt == null || rt.Revoked || rt.ExpiresAt < DateTime.UtcNow)
            return (false, "invalid_token", new { error = "invalid_token" });

        // Rotate: revoke old, issue new
        rt.Revoked = true;

        var newAccessToken = _tokenService.GenerateAccessToken(rt.User);
        var (newPlainRefresh, newRefreshHash) = _tokenService.GenerateRefreshToken();

        var newRt = new RefreshToken
        {
            UserId = rt.UserId,
            TokenHash = newRefreshHash,
            ExpiresAt = _tokenService.GetRefreshTokenExpiry()
        };
        _db.RefreshTokens.Add(newRt);
        await _db.SaveChangesAsync();

        return (true, null, new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newPlainRefresh,
            ExpiresIn = _tokenService.GetAccessTtlMinutes() * 60
        });
    }

    public async Task<bool> LogoutAsync(string plainRefreshToken)
    {
        var tokenHash = _tokenService.HashToken(plainRefreshToken);
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
        if (rt == null || rt.Revoked)
            return false;

        rt.Revoked = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static bool IsValidPassword(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsDigit)) return false;
        return true;
    }
}
