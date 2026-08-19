using System.ComponentModel.DataAnnotations;

namespace App.ServiceDefaults;

public sealed class OpenApiSettings
{
    public const string SectionName = "OpenApi";

    [Required]
    public string DocumentName { get; set; } = "v1";

    [Required]
    public string Title { get; set; } = "API";

    public string Description { get; set; } = string.Empty;

    public bool ExposeInProduction { get; set; }

    public string? OAuthAuthority { get; set; }

    public string? OAuthClientId { get; set; }

    public Dictionary<string, string> Scopes { get; set; } = [];
}
