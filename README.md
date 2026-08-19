# Aspire Starter Platform

A .NET 10 / Aspire starter for building services with vertical slices, Wolverine, EF Core and Keycloak.

## Stack

| Concern | Choice |
|---|---|
| Orchestration | .NET Aspire 13 (`App.AppHost`) |
| API | Wolverine HTTP endpoints, conventions shared via `SharedApi` |
| Versioning | URL segment (`/api/v1/...`) via `Asp.Versioning` |
| Messaging | WolverineFx (handlers, transactional middleware, outbox-ready) |
| Persistence | EF Core 10 + PostgreSQL (Npgsql) |
| Cache | Redis output cache (falls back to in-memory when no `cache` connection string) |
| Identity | Keycloak (JWT bearer, realm imported from `src/App/App.AppHost/Keycloak`) |
| Validation | FluentValidation via Wolverine middleware |
| Mapping | Mapster |
| Errors | RFC 9457 `application/problem+json` |
| Observability | OpenTelemetry + Serilog (console) |

## Run

```bash
dotnet run --project src/App/App.AppHost
```

The dashboard exposes the API, Postgres, Redis and Keycloak. Keycloak listens on `http://localhost:6001`
with realm `starter` and a development user `developer` / `developer`. The realm also ships an
`integration-tests` client with direct access grants, used only by the test suite.

Open Scalar at `/scalar/v1` on the API endpoint, authorize with the `scalar` client, then call the API.

## Layout

```
src/
  App/
    App.AppHost/          Aspire orchestration and container resources
    App.ServiceDefaults/  Telemetry, health checks, OpenAPI, Keycloak auth
  Services/
    Simble/               Sample service: features, data, DI, migrations
  Shared/
    SharedKernel/         Entities, Result, DomainError, ICurrentUser, handler contracts
    SharedApi/            Wolverine HTTP conventions shared by every service
    SharedInfrastructure/ ProblemDetails, paging, sorting, caching, rate limiting
    SharedMessaging/      Wolverine defaults
  Tests/
    Simble.UnitTests/         Pure logic, no external dependencies
    Simble.IntegrationTests/  Full AppHost, requires Docker
```

## Conventions

A feature slice owns its command/query, handler, validator and endpoint in one folder.
Handlers implement a typed contract from `SharedKernel.Abstractions` and never reference HTTP:

```csharp
public sealed class CreateSampleHandler(AppDbContext db)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> ExecuteAsync(CreateSampleCommand command, CancellationToken ct)
    {
        if (await db.Samples.AnyAsync(s => s.Name == command.Name, ct))
        {
            return SampleErrors.NameAlreadyExists(command.Name);
        }
        ...
    }
}
```

Endpoints are a thin Wolverine HTTP surface that calls the handler directly, so the response
type is checked by the compiler rather than at runtime:

```csharp
public static class CreateSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("Create Sample")]
    [EndpointDescription("Create a new sample")]
    [WolverinePost("/samples")]
    public static async Task<IResult> CreateAsync(
        CreateSampleCommand command,
        [NotBody] CreateSampleHandler handler,
        IOutputCacheStore cacheStore,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await handler.ExecuteAsync(command, ct);

        return await result.ToHttpResultAsync(cacheStore, SampleCache.Tag, ct,
            id => Results.Created($"{httpContext.Request.Path}/{id}", id));
    }
}
```

Mark every injected service on a POST/PUT/DELETE endpoint with `[NotBody]`. Wolverine binds the
first unattributed parameter from the request body, so an unmarked handler is deserialized
instead of resolved.

Every route is mounted by `MapPlatformApi()` and requires an authenticated caller through
`RequireAuthorizeOnAll()`. Anonymous access is opt-in per endpoint (`/health/live`,
`/health/ready`, `/openapi`, `/scalar`).

## Database

Entities inheriting `BaseEntity` get audit columns, a soft-delete query filter, and an
application-managed concurrency token (`Version`) that produces `409 Conflict` on lost updates.
Unique indexes on soft-deleted tables must be filtered, as in `SampleConfiguration`:

```csharp
builder.HasIndex(sample => sample.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
```

Add a migration:

```bash
dotnet ef migrations add <Name> --project src/Services/Simble --output-dir Data/Migrations
```

Migrations run at startup by default. Set `Database:Migration:RunOnStartup` to `false` and run them
as a separate job when deploying multiple replicas.

## Platform API conventions

Every service gets the same HTTP behaviour from `SharedApi` with two calls:

```csharp
services.AddPlatformApi();   // registration
app.MapPlatformApi();        // pipeline
```

That applies URL-segment versioning, `RequireAuthorizeOnAll()`, FluentValidation to ProblemDetails,
the per-user output cache policy for endpoints marked `[Cacheable]`, and the 401/403/429/500
response metadata on every chain. Integration tests enforce the conventions across the whole
surface by reading the published OpenAPI document: one proves every operation rejects anonymous
callers, another proves every operation carries tags, a summary and a description. New endpoints
are covered automatically.

## API versioning

Routes are versioned by URL segment. `MapPlatformApi()` sets the template `api/v{version}`, and each
endpoint declares its version:

```csharp
[ApiVersion("1.0")]
[WolverineGet("/samples/{id}")]
```

The endpoint route stays version-free; the policy rewrites it to `/api/v1/samples/{id}`. Every
response carries `api-supported-versions`, and deprecated versions carry `api-deprecated-versions`.

To ship v2 of one endpoint, add a second endpoint class with `[ApiVersion("2.0")]` and the same
route. Both versions coexist; nothing else changes. Retire the old one with a sunset window:

```csharp
app.MapPlatformApi(configure: options => options.UseApiVersioning(versioning =>
{
    versioning.Deprecate("1.0").On(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
    versioning.Sunset("1.0").On(new DateTimeOffset(2027, 6, 1, 0, 0, 0, TimeSpan.Zero));
}));
```

Three integration tests keep this honest: every published path must start with `/api/v`, the
supported-versions header must be emitted, and an unknown version must not route.

## Tests

```bash
dotnet test src/Tests/Simble.UnitTests/Simble.UnitTests.csproj
```

Integration tests boot the real AppHost and need a running Docker daemon.

## Wolverine code generation

Generated handlers under `src/Services/Simble/Internal/Generated` are compiled into the service and
used in production (`TypeLoadMode.Static`). Regenerate after changing a handler signature:

```bash
dotnet run --project src/Services/Simble --no-launch-profile -- codegen write
```

## Behind a reverse proxy

Client IPs drive rate-limit partitions. Register the proxy so `X-Forwarded-For` is trusted:

```json
{ "ForwardedHeaders": { "KnownNetworks": [ "10.0.0.0/8" ] } }
```

## Notes

- Only Postgres keeps a data volume. Keycloak re-imports `starter-realm.json` on every start, so the
  realm stays source-controlled; users created through the Keycloak UI do not survive a restart.
- Rate limiting is per-process. Scale-out multiplies the effective limit; move it to the ingress
  when that matters.
- `Directory.Packages.props` pins versions for the whole platform, including packages not yet
  referenced by any project.
