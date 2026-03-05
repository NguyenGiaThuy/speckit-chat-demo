using IdentityService.Data;
using IdentityService.Data.Entities;
using IdentityService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        return MapToDto(user);
    }

    public async Task<(bool success, UserProfileDto? profile)> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return (false, null);

        if (!string.IsNullOrWhiteSpace(req.DisplayName))
            user.DisplayName = req.DisplayName;

        if (req.AvatarUrl is not null)
            user.AvatarUrl = string.IsNullOrWhiteSpace(req.AvatarUrl) ? null : req.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (true, MapToDto(user));
    }

    private static UserProfileDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        AvatarUrl = user.AvatarUrl,
        CreatedAt = user.CreatedAt
    };
}
