using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Simble.IntegrationTests;

public class ApiVersioningTests(DistributedApplicationFixture fixture)
    : IClassFixture<DistributedApplicationFixture>
{
    [Fact]
    public async Task EveryPublishedPath_IsVersioned()
    {
        var document = await fixture.ApiClient.GetFromJsonAsync<JsonElement>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        var unversioned = document.GetProperty("paths")
            .EnumerateObject()
            .Select(path => path.Name)
            .Where(path => !path.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Empty(unversioned);
    }

    [Fact]
    public async Task VersionedEndpoint_AdvertisesItsSupportedVersions()
    {
        var response = await fixture.AuthenticatedApiClient.GetAsync(
            new Uri("/api/v1/samples", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        Assert.True(
            response.Headers.TryGetValues("api-supported-versions", out var versions),
            "The api-supported-versions header was not emitted.");

        Assert.Contains(versions, version => version.Contains('1', StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnknownVersion_IsNotRouted()
    {
        var response = await fixture.AuthenticatedApiClient.GetAsync(
            new Uri("/api/v99/samples", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
