using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace MessagingService.Services;

/// <summary>Validates a JWT and returns a ClaimsPrincipal, or throws on failure.</summary>
public interface ITokenValidator
{
    ClaimsPrincipal Validate(string token);
}

public class JwtTokenValidator : ITokenValidator
{
    private readonly TokenValidationParameters _params;

    public JwtTokenValidator(TokenValidationParameters validationParams)
    {
        _params = validationParams;
    }

    public ClaimsPrincipal Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, _params, out _);
    }
}
