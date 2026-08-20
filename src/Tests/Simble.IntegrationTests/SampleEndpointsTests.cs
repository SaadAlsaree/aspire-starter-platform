using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Simble.IntegrationTests;

public class SampleEndpointsTests(DistributedApplicationFixture fixture)
    : IClassFixture<DistributedApplicationFixture>
{
    private static readonly Uri SamplesUri = new("/api/v1/samples", UriKind.Relative);

    [Fact]
    public async Task LivenessProbe_IsPubliclyAvailable()
    {
        var response = await fixture.ApiClient.GetAsync(
            new Uri("/health/live", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListSamples_WithoutToken_IsUnauthorized()
    {
        Assert.SkipUnless(fixture.RequiresAuthentication, "The solution was generated without an identity provider.");

        var response = await fixture.ApiClient.GetAsync(
            SamplesUri,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSample_ReturnsCreatedAndIsRetrievable()
    {
        var name = $"sample-{Guid.CreateVersion7()}";

        var id = await CreateSampleAsync(name);
        var response = await fixture.AuthenticatedApiClient.GetAsync(
            new Uri($"/api/v1/samples/{id}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var sample = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        Assert.Equal(name, sample.GetProperty("name").GetString());
    }

    [Fact]
    public async Task CreateSample_WithDuplicateName_ReturnsConflictProblem()
    {
        var name = $"sample-{Guid.CreateVersion7()}";
        await CreateSampleAsync(name);

        var response = await PostSampleAsync(name);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        Assert.Equal("Sample.NameConflict", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateSample_WithInvalidPayload_ReturnsValidationProblem()
    {
        var response = await PostSampleAsync(string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task DeletedSampleName_CanBeReused()
    {
        var name = $"sample-{Guid.CreateVersion7()}";
        var id = await CreateSampleAsync(name);

        var deleteResponse = await fixture.AuthenticatedApiClient.DeleteAsync(
            new Uri($"/api/v1/samples/{id}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var recreated = await PostSampleAsync(name);

        Assert.Equal(HttpStatusCode.Created, recreated.StatusCode);
    }

    [Fact]
    public async Task GetSample_WhenMissing_ReturnsNotFoundProblem()
    {
        var response = await fixture.AuthenticatedApiClient.GetAsync(
            new Uri($"/api/v1/samples/{Guid.CreateVersion7()}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(
            TestContext.Current.CancellationToken);

        Assert.Equal("Sample.NotFound", problem.GetProperty("code").GetString());
    }

    private async Task<Guid> CreateSampleAsync(string name)
    {
        var response = await PostSampleAsync(name);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<Guid>(
            TestContext.Current.CancellationToken);
    }

    private Task<HttpResponseMessage> PostSampleAsync(string name) =>
        fixture.AuthenticatedApiClient.PostAsJsonAsync(
            SamplesUri,
            new { name, description = "created by integration tests" },
            TestContext.Current.CancellationToken);
}
