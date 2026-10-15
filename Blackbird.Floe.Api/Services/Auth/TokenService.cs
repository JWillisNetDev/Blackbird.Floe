using Microsoft.Extensions.Options;
using Blackbird.Floe.Data.Models.Identity;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Blackbird.Floe.Api.Services.Auth;

public class TokenService
{
    private readonly IOptions<JwtOptions> _options;
    private JwtOptions Options => _options.Value;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string Create(User user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(Options.SigningKey));
        JwtSecurityToken token = new(
            Options.Issuer,
            Options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(Options.LifetimeMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        
        return _jwtSecurityTokenHandler.WriteToken(token);
    }
}