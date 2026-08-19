using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SharedKernel.Abstractions;

namespace SharedInfrastructure.Security;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? Id =>
        Guid.TryParse(Principal?.FindFirstValue("sub"), out var id) ? id : null;

    public string? Name =>
        Principal?.FindFirstValue("preferred_username") ?? Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
