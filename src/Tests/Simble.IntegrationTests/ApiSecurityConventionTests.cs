using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Simble.IntegrationTests;

/// <summary>
/// Walks every operation the service publishes and proves it rejects anonymous callers.
/// A new endpoint is covered the moment it is mapped, without touching this test.
/// </summary>
public class ApiSecurityConventionTests(DistributedApplicationFixture fixture)
    : IClassFixture<DistributedApplicationFixture>
{
    private static readonly string[] AnonymousPathPrefixes =
        ["/health", "/openapi", "/scalar"];

    private static readonly string[] HttpMethods =
        ["get", "post", "put", "delete", "patch"];

    [Fact]
    public async Task EveryPublishedOperation_RejectsAnonymousCallers()
    {
        var operations = await DiscoverOperationsAsync();

        Assert.NotEmpty(operations);

        var anonymouslyReachable = new List<string>();

        foreach (var (method, path) in operations)
        {
            using var request = new HttpRequestMessage(
                new HttpMethod(method),
                new Uri(path, UriKind.Relative));

            var response = await fixture.ApiClient.SendAsync(
                request,
                TestContext.Current.CancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                anonymouslyReachable.Add($"{method.ToUpperInvariant()} {path} -> {(int)response.StatusCode}");
            }
        }

        Assert.Empty(anonymouslyReachable);
    }

    [Fact]
    public async Task EveryPublishedOperation_IsDocumented()
    {
        var document = await fixture.ApiClient.GetFromJsonAsync<JsonElement>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        var undocumented = new List<string>();

        foreach (var path in document.GetProperty("paths").EnumerateObject())
        {
            if (AnonymousPathPrefixes.Any(prefix =>
                path.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            foreach (var method in HttpMethods)
            {
                if (!path.Value.TryGetProperty(method, out var operation))
                {
                    continue;
                }

                if (IsMissingText(operation, "summary")
                    || IsMissingText(operation, "description")
                    || !operation.TryGetProperty("tags", out var tags)
                    || tags.GetArrayLength() == 0)
                {
                    undocumented.Add($"{method.ToUpperInvariant()} {path.Name}");
                }
            }
        }

        Assert.Empty(undocumented);
    }

    private static bool IsMissingText(JsonElement operation, string property) =>
        !operation.TryGetProperty(property, out var value)
        || string.IsNullOrWhiteSpace(value.GetString());

    private static string SubstituteRouteParameters(string path)
    {
        while (path.Contains('{', StringComparison.Ordinal))
        {
            var start = path.IndexOf('{', StringComparison.Ordinal);
            var end = path.IndexOf('}', start);

            path = string.Concat(
                path.AsSpan(0, start),
                Guid.CreateVersion7().ToString(),
                path.AsSpan(end + 1));
        }

        return path;
    }

    private async Task<List<(string Method, string Path)>> DiscoverOperationsAsync()
    {
        var document = await fixture.ApiClient.GetFromJsonAsync<JsonElement>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken);

        var operations = new List<(string, string)>();

        foreach (var path in document.GetProperty("paths").EnumerateObject())
        {
            if (AnonymousPathPrefixes.Any(prefix =>
                path.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            foreach (var method in HttpMethods.Where(method =>
                path.Value.TryGetProperty(method, out _)))
            {
                operations.Add((method, SubstituteRouteParameters(path.Name)));
            }
        }

        return operations;
    }
}
