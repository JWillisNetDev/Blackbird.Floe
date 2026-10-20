using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Blackbird.Floe.Api.Services;
using Blackbird.Floe.Api.Services.Auth;
using Blackbird.Floe.Data;
using Blackbird.Floe.Data.Models.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using OpenTelemetry.Trace;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi(o =>
    {
        o.AddDocumentTransformer(async (doc, _, _) =>
        {
            doc.Components ??= new OpenApiComponents();
            doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };
        });

        o.AddOperationTransformer(async (op, ctx, _) =>
        {
            var metadata = ctx.Description.ActionDescriptor.EndpointMetadata;
            if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
            {
                return;
            }

            op.Security ??= [];
            op.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", ctx.Document)] = [],
            });
        });
    });
}

// builder.AddDefaultHealthChecks();
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<FloeDb>("floe", configureDbContextOptions: opt =>
{
    string migrationsAssembly = typeof(FloeDb).Assembly.GetName().Name!;
    opt.UseNpgsql(n => n.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddValidation();

builder.Services.AddAppAuth(builder.Configuration);

builder.Services.AddScoped<IUserManager, FloeDbUserManager>();
builder.Services.AddScoped<PasswordHashService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddPreferredSecuritySchemes("Bearer"));
}

await using (var scope = app.Services.CreateAsyncScope())
{
    using var db = scope.ServiceProvider.GetRequiredService<FloeDb>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

// TODO - ALL endpoint mapping goes into extensions and static classes
// handlers also go into static classes

var api = app.MapGroup("/api");
var auth = app.MapGroup("/auth");

auth.MapPost("/login", async Task<Results<Ok<LoginResponse>, ValidationProblem, ProblemHttpResult>> (LoginRequest req, IUserManager users, TokenService tokens, PasswordHashService passwordHasher) =>
{
    if (await users.FindByEmailAsync(req.Email) is { PasswordHash: string expectedHash } user
        && passwordHasher.Verify(req.Password, expectedHash))
    {
        return TypedResults.Ok(new LoginResponse(tokens.Create(user)));
    }

    return TypedResults.Problem("The username or password was not registered or incorrect.", statusCode: StatusCodes.Status401Unauthorized);
})
    .WithSummary("Log a User in")
    .ProducesProblem(StatusCodes.Status401Unauthorized);

auth.MapPost("/register", async Task<Results<Ok<LoginResponse>, ValidationProblem, ProblemHttpResult>> (RegisterRequest req, IUserManager users, TokenService tokens, PasswordHashService passwordHasher) =>
{
    // first, ensure the email is unique
    if (await users.EmailExistsAsync(req.Email))
    {
        return TypedResults.Problem("The email is already registered.", statusCode: StatusCodes.Status409Conflict);
    }

    // hash the password, create the user, and save it to the database
    var passwordHash = passwordHasher.Hash(req.Password);
    User user = new(req.Email, passwordHash);
    await users.AddAsync(user);
    return TypedResults.Ok(new LoginResponse(tokens.Create(user)));
})
    .WithSummary("Register a New User")
    .ProducesProblem(StatusCodes.Status409Conflict);

app.MapGet("/api/weather", () => new[] { "Sunny", "Cloudy", "Rainy" });

app.Run();

public record LoginRequest([Required, EmailAddress] string Email, [Required, PasswordPropertyText] string Password);
public record LoginResponse(string AccessToken);
public record RegisterRequest([Required, EmailAddress] string Email, [Required, PasswordPropertyText] string Password);