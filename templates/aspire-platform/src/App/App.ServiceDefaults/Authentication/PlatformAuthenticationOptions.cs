namespace App.ServiceDefaults.Authentication;

public sealed class PlatformAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public AuthenticationProvider Provider { get; set; } = AuthenticationProvider.None;

    public KeycloakAuthenticationOptions Keycloak { get; set; } = new();

    public EntraIdAuthenticationOptions EntraId { get; set; } = new();
}
