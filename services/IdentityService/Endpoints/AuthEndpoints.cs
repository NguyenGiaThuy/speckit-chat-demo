using IdentityService.DTOs;
using IdentityService.Services;

namespace IdentityService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (RegisterRequest req, AuthService authService) =>
        {
            var (success, error, result) = await authService.RegisterAsync(req);
            return success
                ? Results.Created("/users/me", result)
                : error == "email_taken"
                    ? Results.Conflict(result)
                    : Results.UnprocessableEntity(result);
        });

        group.MapPost("/login", async (LoginRequest req, AuthService authService) =>
        {
            var (success, error, result) = await authService.LoginAsync(req);
            return success
                ? Results.Ok(result)
                : Results.Unauthorized();
        });

        group.MapPost("/refresh", async (RefreshRequest req, AuthService authService) =>
        {
            var (success, error, result) = await authService.RefreshAsync(req.RefreshToken);
            return success
                ? Results.Ok(result)
                : Results.Unauthorized();
        });

        group.MapPost("/logout", async (LogoutRequest req, AuthService authService, HttpContext ctx) =>
        {
            await authService.LogoutAsync(req.RefreshToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
