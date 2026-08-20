namespace App.ServiceDefaults.Authentication;

public sealed class EntraIdAuthenticationOptions
{
    /// <summary>Gets or sets the directory (tenant) id, or "organizations" for multi-tenant apps.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Gets or sets the application ID URI or client id the token must be issued for.</summary>
    public string Audience { get; set; } = string.Empty;

    public string Instance { get; set; } = "https://login.microsoftonline.com";

    public bool RequireHttpsMetadata { get; set; } = true;

    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>Gets or sets issuer overrides. Required for multi-tenant apps.</summary>
    public List<string> ValidIssuers { get; set; } = [];

    public string Authority =>
        $"{Instance.TrimEnd('/')}/{TenantId}/v2.0";
}
