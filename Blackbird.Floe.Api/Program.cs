using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Blackbird.Floe.Api.Services;
using Blackbird.Floe.Api.Services.Auth;
using Blackbird.Floe.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.AddDefaultHealthChecks();
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<FloeDb>("floe", configureDbContextOptions: opt =>
{
    string migrationsAssembly = typeof(FloeDb).Assembly.GetName().Name!;
    opt.UseNpgsql(n => n.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddAppAuth(builder.Configuration);

builder.Services.AddScoped<IUserManager, FloeDbUserManager>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    using var db = scope.ServiceProvider.GetRequiredService<FloeDb>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", (LoginRequest req, IUserManager users, TokenService tokens) =>
{
    
});

app.MapDefaultEndpoints();

app.MapGet("/api/weather", () => new[] { "Sunny", "Cloudy", "Rainy" });

app.Run();

public record LoginRequest([Required] string Email, [Required, PasswordPropertyText] string Password);
