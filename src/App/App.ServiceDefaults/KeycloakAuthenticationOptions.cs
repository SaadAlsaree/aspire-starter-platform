using System.ComponentModel.DataAnnotations;

namespace App.ServiceDefaults;

public sealed class KeycloakAuthenticationOptions
{
    public const string SectionName = "Authentication:Keycloak";

    [Required]
    public string ServiceName { get; set; } = "keycloak";

    [Required]
    public string Realm { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public string? Authority { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    public List<string> ValidIssuers { get; set; } = [];
}
