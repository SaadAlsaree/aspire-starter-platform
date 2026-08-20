# SharedKernel

Domain primitives for the [Aspire Starter Platform](https://github.com/saad-alsaree/aspire-starter-platform).

- `Result` / `Result<T>` and `DomainError` with an `ErrorType` that maps cleanly onto HTTP status codes.
- `BaseEntity` with audit columns, soft delete and an application-managed concurrency token.
- `ICommandHandler<,>` / `IQueryHandler<,>` so handler return types are checked by the compiler.
- `ICurrentUser` for auditing without leaking `HttpContext` into the domain.

```csharp
public sealed class CreateSampleHandler(AppDbContext db)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> ExecuteAsync(CreateSampleCommand command, CancellationToken ct)
    {
        if (await db.Samples.AnyAsync(s => s.Name == command.Name, ct))
        {
            return DomainError.Conflict("Sample.NameConflict", "Name already exists.");
        }
        ...
    }
}
```
