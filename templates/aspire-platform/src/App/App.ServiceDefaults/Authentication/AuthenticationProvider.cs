namespace App.ServiceDefaults.Authentication;

public enum AuthenticationProvider
{
    /// <summary>No authentication. Endpoints are reachable anonymously.</summary>
    None = 0,

    /// <summary>Keycloak realm issuing JWT bearer tokens.</summary>
    Keycloak = 1,

    /// <summary>Microsoft Entra ID issuing JWT bearer tokens.</summary>
    EntraId = 2,
}
