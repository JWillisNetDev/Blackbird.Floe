using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Blackbird.Floe.Api.Services.Auth;

public static class AuthExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppAuth(IConfiguration config)
        {
            var section = config.GetSection("Jwt");
            services.Configure<JwtOptions>(section);
            JwtOptions options = section.Get<JwtOptions>()!;

            services.AddSingleton<TokenService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => o.TokenValidationParameters = new()
                {
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                });
            services.AddAuthorizationBuilder().AddPolicy(Roles.Admin, p => p.RequireRole(Roles.Admin));

            return services;
        }
    }

    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        public TBuilder RequireAdmin() => builder.RequireAuthorization(Roles.Admin);
    }
}