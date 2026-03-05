using IdentityService.Data;
using IdentityService.Data.Entities;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _userService = new UserService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedUser(
        string email = "user@example.com",
        string displayName = "Test User",
        string? avatarUrl = null)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
            DisplayName = displayName,
            AvatarUrl = avatarUrl
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetProfile_ExistingUser_ReturnsProfile()
    {
        var user = await SeedUser("get@example.com", "Get User");

        var profile = await _userService.GetProfileAsync(user.Id);

        Assert.NotNull(profile);
        Assert.Equal(user.Id, profile.Id);
        Assert.Equal("get@example.com", profile.Email);
        Assert.Equal("Get User", profile.DisplayName);
    }

    [Fact]
    public async Task GetProfile_UnknownId_ReturnsNull()
    {
        var profile = await _userService.GetProfileAsync(Guid.NewGuid());
        Assert.Null(profile);
    }

    [Fact]
    public async Task UpdateProfile_DisplayName_UpdatesCorrectly()
    {
        var user = await SeedUser();

        var (success, profile) = await _userService.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            DisplayName = "Updated Name"
        });

        Assert.True(success);
        Assert.NotNull(profile);
        Assert.Equal("Updated Name", profile.DisplayName);

        var dbUser = await _db.Users.FindAsync(user.Id);
        Assert.Equal("Updated Name", dbUser!.DisplayName);
    }

    [Fact]
    public async Task UpdateProfile_AvatarUrl_UpdatesCorrectly()
    {
        var user = await SeedUser();

        var (success, profile) = await _userService.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            AvatarUrl = "https://example.com/avatar.png"
        });

        Assert.True(success);
        Assert.Equal("https://example.com/avatar.png", profile!.AvatarUrl);
    }

    [Fact]
    public async Task UpdateProfile_ClearAvatarUrl_SetsToNull()
    {
        var user = await SeedUser(avatarUrl: "https://example.com/old.png");

        var (success, profile) = await _userService.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            AvatarUrl = "" // empty string clears the avatar
        });

        Assert.True(success);
        Assert.Null(profile!.AvatarUrl);
    }

    [Fact]
    public async Task UpdateProfile_EmptyDisplayName_DoesNotUpdate()
    {
        var user = await SeedUser(displayName: "Original Name");

        var (success, profile) = await _userService.UpdateProfileAsync(user.Id, new UpdateProfileRequest
        {
            DisplayName = "" // blank should not update display name
        });

        Assert.True(success);
        Assert.Equal("Original Name", profile!.DisplayName);
    }

    [Fact]
    public async Task UpdateProfile_UnknownUser_ReturnsFalse()
    {
        var (success, profile) = await _userService.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest
        {
            DisplayName = "Nobody"
        });

        Assert.False(success);
        Assert.Null(profile);
    }
}
