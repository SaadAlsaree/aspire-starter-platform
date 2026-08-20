# SharedMessaging

Opinionated [WolverineFx](https://wolverinefx.net) defaults for the
[Aspire Starter Platform](https://github.com/saad-alsaree/aspire-starter-platform).

- Application assembly discovery and service name derived from the host assembly.
- `TypeLoadMode.Auto` in development, `Static` elsewhere, so production uses pre-generated code.
- FluentValidation middleware and a service-location policy that fails fast on accidental use.

```csharp
builder.Host.AddWolverineDefaults(builder.Environment, typeof(Program).Assembly, options =>
{
    options.Policies.AutoApplyTransactions();
});
```
