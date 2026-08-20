var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("simbledb", "simble");

var cache = builder.AddRedis("cache");

var simble = builder.AddProject<Projects.Simble>("simble")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithExternalHttpEndpoints();

// Only the Keycloak provider needs a local identity container. Entra ID uses the cloud
// tenant, and None has no identity provider at all.
if (string.Equals(builder.Configuration["Authentication:Provider"], "Keycloak", StringComparison.OrdinalIgnoreCase))
{
    var keycloak = builder.AddKeycloak("keycloak", 6001)
        .WithRealmImport("./Keycloak");

    simble.WithReference(keycloak).WaitFor(keycloak);
}

await builder.Build().RunAsync();
