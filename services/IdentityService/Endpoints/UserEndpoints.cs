using System.Security.Claims;
using IdentityService.DTOs;
using IdentityService.Services;

namespace IdentityService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal user, UserService userService) =>
        {
            var userId = GetUserId(user);
            if (userId == null) return Results.Unauthorized();

            var profile = await userService.GetProfileAsync(userId.Value);
            return profile != null ? Results.Ok(profile) : Results.NotFound();
        });

        group.MapPut("/me", async (ClaimsPrincipal user, UpdateProfileRequest req, UserService userService) =>
        {
            var userId = GetUserId(user);
            if (userId == null) return Results.Unauthorized();

            var (success, profile) = await userService.UpdateProfileAsync(userId.Value, req);
            return success ? Results.Ok(profile) : Results.NotFound();
        });
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
