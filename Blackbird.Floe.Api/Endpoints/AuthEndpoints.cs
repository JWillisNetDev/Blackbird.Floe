using System.Security.Cryptography.X509Certificates;

using Blackbird.Floe.Api.Dtos;
using Blackbird.Floe.Api.Services.Auth;
using Blackbird.Floe.Data.Models.Identity;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Blackbird.Floe.Api.Endpoints;

public static class AuthEndpoints
{
    public static async Task<Results<Ok<TokenResponse>, ValidationProblem, ProblemHttpResult>> Login(
        LoginRequest request,
        IUserManager users,
        TokenService tokens,
        PasswordHashService passwordHasher)
    {
        if (await users.FindByEmailAsync(request.Email) is { PasswordHash: string passwordHash } user
            && passwordHasher.Verify(request.Password, passwordHash))
        {
            string token = tokens.Create(user);
            return TypedResults.Ok(new TokenResponse(token));
        }

        return TypedResults.Problem("The email is not registered or the password is incorrect.", statusCode: StatusCodes.Status401Unauthorized);
    }

    public static async Task<Results<Ok<TokenResponse>, ValidationProblem, ProblemHttpResult>> Register(
        LoginRequest request,
        IUserManager users,
        TokenService tokens,
        PasswordHashService passwordHasher)
    {
        if (await users.EmailExistsAsync(request.Email))
        {
            return TypedResults.Problem("The email is already registered.", statusCode: StatusCodes.Status409Conflict);
        }

        string passwordHash = passwordHasher.Hash(request.Password);
        User user = new(request.Email, passwordHash);
        await users.AddAsync(user);
        string token = tokens.Create(user);
        return TypedResults.Ok(new TokenResponse(token));
    }

    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointRouteBuilder
    {
        public TBuilder AddAuthEndpoints()
        {
            builder.MapPost("/login", AuthEndpoints.Login)
                .WithSummary("Login")
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            builder.MapPost("/register", AuthEndpoints.Register)
                .WithSummary("Register")
                .ProducesProblem(StatusCodes.Status409Conflict);

            return builder;
        }
    }
}
