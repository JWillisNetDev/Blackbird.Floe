using System.Reflection;

using Blackbird.Floe.Api.Endpoints;
using Blackbird.Floe.Api.Services;
using Blackbird.Floe.Api.Services.Auth;
using Blackbird.Floe.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

bool isOpenApiGeneration = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

var builder = WebApplication.CreateBuilder(args);

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

// fixes: OpenAPI spec gen shouldn't try to migrate, but will because it runs the program.
if (!isOpenApiGeneration)
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        using var db = scope.ServiceProvider.GetRequiredService<FloeDb>();
        await db.Database.MigrateAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

var api = app.MapGroup("/api");
var auth = api.MapGroup("/auth");

auth.AddAuthEndpoints();

app.MapGet("/api/weather", () => new[] { "Sunny", "Cloudy", "Rainy" });

app.Run();