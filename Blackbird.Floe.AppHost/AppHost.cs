using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("floe");

var api = builder.AddProject<Blackbird_Floe_Api>("api")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.AddViteApp("web", "../floe-web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT"); // Use Aspire's default port

builder.Build().Run();
