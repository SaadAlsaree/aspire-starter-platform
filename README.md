# Aspire Starter Platform

**Ship a production-shaped .NET API in one command — not in three weeks.**

A .NET 10 Aspire template that gives you the parts every serious service needs and most starters
leave out: authentication, versioning, caching, rate limiting, problem details, observability,
soft delete with audit and optimistic concurrency — already wired, already tested.

```bash
dotnet new install AspirePlatform.Templates
dotnet new aspire-platform -n Contoso --ServiceName Billing
dotnet run --project Contoso/src/App/App.AppHost
```

That is a running API, PostgreSQL, Redis and Keycloak, with a signed-in developer account and a
working CRUD feature. No manual wiring.

---

## Why teams pick it

| | |
|---|---|
| **Mistakes caught at compile time** | Endpoints call handlers directly through typed contracts. Change a handler's return type and the build breaks — not production. |
| **Security you cannot forget** | Deny-by-default authorization plus a test that walks every published endpoint and fails the build if one is reachable anonymously. |
| **Scales to hundreds of endpoints** | Conventions live in one shared module. A new endpoint inherits versioning, auth, caching and error handling without touching infrastructure. |
| **Your identity, your choice** | Keycloak, Microsoft Entra ID, or none — a config switch, not a rewrite. |
| **Proven, not promised** | 44 tests ship green, including integration tests that boot the real stack in Docker. |

## The core idea

Business logic never touches HTTP. The endpoint is a thin, compiler-checked adapter.

```csharp
public sealed class CreateSampleHandler(AppDbContext db)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> ExecuteAsync(CreateSampleCommand command, CancellationToken ct)
    {
        if (await db.Samples.AnyAsync(s => s.Name == command.Name, ct))
        {
            return SampleErrors.NameAlreadyExists(command.Name);   // becomes 409 + problem details
        }
        ...
    }
}
```

```csharp
[ApiVersion("1.0")]
[Tags("Samples")]
[EndpointSummary("Create Sample")]
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
```

`ErrorType` maps to the status code centrally, so 404, 409 and 400 are never decided by hand.

## Enforced automatically

Five tests read the service's own OpenAPI document and hold the whole surface to the rules. New
endpoints are covered the moment they are mapped.

- Every endpoint rejects anonymous callers
- Every endpoint carries tags, a summary and a description
- Every route is versioned under `/api/v{n}`
- Supported versions are advertised; unknown versions do not route
- Wolverine's generated code is current (CI fails on drift)

## Identity providers

Set `Authentication:Provider`, or pass `--AuthProvider` when generating.

| Provider | Local footprint | Notes |
|---|---|---|
| `Keycloak` | Container started by the AppHost with an imported realm | Default. Works offline |
| `EntraId` | None — validated against your tenant | Set `EntraId:TenantId` and `Audience` first |
| `None` | None | Anonymous endpoints, for spikes |

`ICurrentUser` reads `sub` then `oid`, so audit columns fill in under both Keycloak and Entra ID.

## Templates

| Template | Short names | Creates |
|---|---|---|
| Aspire Vertical Slice Platform | `aspire-platform`, `avsp` | The whole solution |
| Aspire Vertical Slice Service | `aspire-service`, `avss` | Another service inside it |

### Create a solution

```bash
dotnet new aspire-platform -n Contoso --ServiceName Billing --AuthProvider EntraId
```

Solution name and service name are independent.

| Option | Default |
|---|---|
| `--ServiceName` | `Api` |
| `--AuthProvider` | `Keycloak` |
| `--KeycloakPort` | `6001` |
| `--CorsOrigin` | `http://localhost:3000` |
| `--IncludeCiWorkflow` | `true` |

### Add a service

Run from the solution's `src/Services` folder so the shared project references resolve:

```bash
cd src/Services
dotnet new aspire-service -n Inventory
dotnet sln ../../Contoso.slnx add Inventory/Inventory.csproj
dotnet add ../App/App.AppHost reference Inventory
```

Register the resource in `src/App/App.AppHost/AppHost.cs`:

```csharp
var inventoryDb = postgres.AddDatabase("inventorydb", "inventory");

var inventory = builder.AddProject<Projects.Inventory>("inventory")
    .WithReference(inventoryDb).WaitFor(inventoryDb)
    .WithReference(cache).WaitFor(cache)
    .WithExternalHttpEndpoints();
```

With the Keycloak provider, add `inventory.WithReference(keycloak).WaitFor(keycloak);` inside the
existing `if` block, and copy the `<service>-api` client and audience mapper in
`Keycloak/starter-realm.json` for `inventory-api`.

That is all. The new service ships with its own migrations and generated endpoints, so it runs
immediately. Regenerate only after you change the model or an endpoint signature:

```bash
dotnet ef migrations add <Name> --project src/Services/Inventory --output-dir Data/Migrations
dotnet run --project src/Services/Inventory --no-launch-profile -- codegen write
```

The template prints these steps after it runs.

## Under the hood

| Concern | Choice |
|---|---|
| Orchestration | .NET Aspire 13 |
| API | Wolverine HTTP, vertical slices, conventions in `SharedApi` |
| Versioning | URL segment via `Asp.Versioning` |
| Persistence | EF Core 10 + PostgreSQL, soft delete, audit, concurrency token |
| Cache | Redis output cache, per-user, tag eviction on writes |
| Errors | RFC 9457 `application/problem+json` |
| Validation | FluentValidation |
| Observability | OpenTelemetry + Serilog |
| Tests | xUnit v3, Aspire integration testing |

## Operating notes

- **`[NotBody]`** — mark injected services on POST/PUT/DELETE endpoints. Wolverine binds the first
  unattributed parameter from the request body.
- **Code generation** — after changing an endpoint signature run
  `dotnet run --project src/Services/<Service> --no-launch-profile -- codegen write`.
- **Migrations** — run at startup by default; set `Database:Migration:RunOnStartup` to `false` and
  run them as a job when deploying multiple replicas.
- **Behind a proxy** — register it so `X-Forwarded-For` is trusted, otherwise every caller shares
  one rate-limit partition: `{ "ForwardedHeaders": { "KnownNetworks": [ "10.0.0.0/8" ] } }`.
- **Rate limiting** is per process. Scale-out multiplies the effective limit; move it to the ingress
  when that matters.

## Contributing to the template

The published template is generated from this solution, so what ships is exactly what CI builds and
tests.

```powershell
./sync-template.ps1 -Check    # fails if the packaged copy drifted
./pack.ps1 -Smoke             # pack, install, generate a throwaway solution and build it
./pack.ps1 -Push -ApiKey <key>
```

MIT licensed.
