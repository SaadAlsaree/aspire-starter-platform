# SharedInfrastructure

Cross-cutting infrastructure for the [Aspire Starter Platform](https://github.com/saad-alsaree/aspire-starter-platform).

- RFC 9457 `application/problem+json` for every failure, including a `code` extension and `traceId`.
- `ToHttpResult()` translating `Result` / `Result<T>` into responses without endpoints guessing status codes.
- `PagedList` and `ApplySort` with a tie-breaker so paging stays stable.
- Per-user output caching, rate limiting partitioned by user or client IP, and forwarded-headers configuration.
- `MigrateDatabaseAsync<TContext>()` with an opt-out for multi-replica deployments.

```csharp
services.AddApplicationProblemDetails();
services.AddApplicationRateLimiting(configuration);
services.AddApplicationForwardedHeaders(configuration);
services.AddCurrentUser();
```
