using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Simble.IntegrationTests;

public sealed class DistributedApplicationFixture : IAsyncLifetime
{
    private const string Realm = "starter";
    private const string ClientId = "integration-tests";
    private const string UserName = "developer";
    private const string Password = "developer";

    private DistributedApplication? application;

    public HttpClient ApiClient { get; private set; } = null!;

    public HttpClient AuthenticatedApiClient { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.App_AppHost>();

        application = await builder.BuildAsync();
        await application.StartAsync();

        using var startup = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await application.ResourceNotifications
            .WaitForResourceHealthyAsync("simble", startup.Token);

        ApiClient = application.CreateHttpClient("simble");
        AuthenticatedApiClient = application.CreateHttpClient("simble");
        AuthenticatedApiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await RequestAccessTokenAsync(startup.Token));
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();
        AuthenticatedApiClient?.Dispose();

        if (application is not null)
        {
            await application.DisposeAsync();
        }
    }

    private async Task<string> RequestAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var keycloak = application!.CreateHttpClient("keycloak");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["username"] = UserName,
            ["password"] = Password,
            ["scope"] = "openid",
        });

        var response = await keycloak.PostAsync(
            new Uri($"/realms/{Realm}/protocol/openid-connect/token", UriKind.Relative),
            form,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken);

        return payload.GetProperty("access_token").GetString()!;
    }
}
