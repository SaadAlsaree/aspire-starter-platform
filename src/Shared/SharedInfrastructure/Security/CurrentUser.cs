using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SharedKernel.Abstractions;

namespace SharedInfrastructure.Security;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    // Keycloak puts a guid in "sub"; Entra ID puts an opaque value there and the guid in "oid".
    public Guid? Id => ReadGuidClaim("sub") ?? ReadGuidClaim("oid");

    public string? Name =>
        Principal?.FindFirstValue("preferred_username") ?? Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    private Guid? ReadGuidClaim(string claimType) =>
        Guid.TryParse(Principal?.FindFirstValue(claimType), out var value) ? value : null;
}
