using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SharedInfrastructure.RateLimiting;

internal static class RateLimitPartitionKey
{
    public static string ForUserOrClient(HttpContext context)
    {
        var userId = context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return context.User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : ForClient(context);
    }

    public static string ForClient(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is null)
        {
            return $"connection:{context.Connection.Id}";
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        return $"ip:{address}";
    }
}
