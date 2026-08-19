using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SharedInfrastructure.Security;
using SharedKernel.Abstractions;

namespace Simble.UnitTests;

public class CurrentUserTests
{
    [Fact]
    public void Id_ReadsSubjectClaim()
    {
        var id = Guid.CreateVersion7();

        var currentUser = CreateCurrentUser(
            new Claim("sub", id.ToString()),
            new Claim("preferred_username", "developer"));

        Assert.Equal(id, currentUser.Id);
        Assert.Equal("developer", currentUser.Name);
        Assert.True(currentUser.IsAuthenticated);
    }

    [Fact]
    public void Id_IsNullWhenSubjectIsNotAGuid()
    {
        var currentUser = CreateCurrentUser(new Claim("sub", "service-account"));

        Assert.Null(currentUser.Id);
    }

    [Fact]
    public void IsAuthenticated_IsFalseWithoutHttpContext()
    {
        var currentUser = new ServiceCollection()
            .AddCurrentUser()
            .BuildServiceProvider()
            .GetRequiredService<ICurrentUser>();

        Assert.Null(currentUser.Id);
        Assert.False(currentUser.IsAuthenticated);
    }

    private static ICurrentUser CreateCurrentUser(params Claim[] claims)
    {
        var provider = new ServiceCollection().AddCurrentUser().BuildServiceProvider();

        provider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")),
            };

        return provider.GetRequiredService<ICurrentUser>();
    }
}
