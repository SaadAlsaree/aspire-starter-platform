var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("simbledb", "simble");

var cache = builder.AddRedis("cache");

var keycloak = builder.AddKeycloak("keycloak", 6001)
    .WithRealmImport("./Keycloak");

builder.AddProject<Projects.Simble>("simble")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
