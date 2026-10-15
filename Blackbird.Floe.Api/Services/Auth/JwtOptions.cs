namespace Blackbird.Floe.Api.Services.Auth;

public class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
    public int LifetimeMinutes { get; init; } = 30;
}
