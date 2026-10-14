using Blackbird.Floe.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.AddDefaultHealthChecks();
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<FloeDb>("floe");

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    using var db = scope.ServiceProvider.GetRequiredService<FloeDb>();
    // Uncomment when migrations are added
    // await db.Database.MigrateAsync(CancellationToken.None);
}

app.MapDefaultEndpoints();

app.MapGet("/api/weather", () => new[] { "Sunny", "Cloudy", "Rainy" });

app.Run();
