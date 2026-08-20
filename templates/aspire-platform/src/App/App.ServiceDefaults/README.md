# App.ServiceDefaults

Aspire service defaults for the
[Aspire Starter Platform](https://github.com/saad-alsaree/aspire-starter-platform), shipped as a
package instead of a per-solution copied project.

- OpenTelemetry traces, metrics and logs with OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.
- `/health/live` and `/health/ready` probes, service discovery and standard HTTP resilience.
- OpenAPI with Scalar, including the Keycloak OAuth flow.
- Keycloak JWT bearer authentication bound from `Authentication:Keycloak`.

```csharp
builder.AddServiceDefaults();
...
app.MapDefaultEndpoints();
app.MapDefaultOpenApi();
```
