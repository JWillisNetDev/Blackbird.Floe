using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
    
var floedb = postgres.AddDatabase("floe");

var api = builder.AddProject<Blackbird_Floe_Api>("api")
    .WithUrlForEndpoint("https", static u =>
    {
        u.Url = "/scalar";
        u.DisplayText = "Scalar UI";
    })
    .WithReference(floedb)
    .WaitFor(floedb);

builder.AddViteApp("web", "../floe-web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT"); // Use Aspire's default port

builder.Build().Run();
