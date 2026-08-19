# SharedApi

Wolverine HTTP conventions shared by every service on the
[Aspire Starter Platform](https://github.com/saad-alsaree/aspire-starter-platform).

Two calls give a service its entire HTTP contract:

```csharp
services.AddPlatformApi();   // registration
app.MapPlatformApi();        // pipeline
```

That applies URL-segment API versioning (`/api/v{version}/...`), `RequireAuthorizeOnAll()`,
FluentValidation to ProblemDetails, a per-user output cache policy for endpoints marked
`[Cacheable]`, and 401/403/429/500 response metadata on every chain.

```csharp
[ApiVersion("1.0")]
[Tags("Samples")]
[EndpointSummary("Get Sample")]
[WolverineGet("/samples/{id}")]
[Cacheable("samples")]
public static async Task<IResult> GetAsync(
    Guid id,
    [NotBody] GetByIdSampleHandler handler,
    CancellationToken ct) =>
    (await handler.ExecuteAsync(new GetByIdSampleQuery(id), ct)).ToHttpResult();
```

Mark injected services on POST/PUT/DELETE with `[NotBody]`; Wolverine binds the first
unattributed parameter from the request body.
