namespace App.ServiceDefaults.Authentication;

public sealed class KeycloakAuthenticationOptions
{
    public string ServiceName { get; set; } = "keycloak";

    public string Realm { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Gets or sets a fixed authority that bypasses Aspire service discovery.</summary>
    public string? Authority { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    public List<string> ValidIssuers { get; set; } = [];
}
