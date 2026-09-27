using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EnergyPlatform.Api.Tests;

[Collection(PlatformCollection.Name)]
public sealed class AccessTests(PlatformFixture platform)
{
    [Fact]
    public async Task Health_reports_the_database_as_reachable()
    {
        using var client = platform.AnonymousClient();

        var health = await client.GetFromJsonAsync<JsonNode>("/health");

        Assert.Equal("healthy", health!["status"]!.GetValue<string>());
        Assert.True(health["databaseReachable"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("/api/meters")]
    [InlineData("/api/anomalies")]
    [InlineData("/api/dashboard/summary")]
    public async Task Data_endpoints_reject_requests_without_a_session(string path)
    {
        using var client = platform.AnonymousClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Public_configuration_announces_demo_mode_without_a_session()
    {
        using var client = platform.AnonymousClient();

        var config = await client.GetFromJsonAsync<JsonNode>("/api/config");

        Assert.Equal("DEMO", config!["authMode"]!.GetValue<string>());
    }

    [Fact]
    public async Task Login_rejects_a_wrong_password()
    {
        using var client = platform.AnonymousClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "demo@vatio.app", password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_signed_in_client_can_read_data()
    {
        using var client = await platform.SignedInClientAsync();

        var response = await client.GetAsync("/api/meters");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
