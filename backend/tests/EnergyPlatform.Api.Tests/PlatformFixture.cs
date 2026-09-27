using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace EnergyPlatform.Api.Tests;

public sealed class PlatformFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private readonly SemaphoreSlim analysisLock = new(1, 1);
    private WebApplicationFactory<Program>? factory;
    private JsonNode? completedAnalysis;

    public WebApplicationFactory<Program> Factory => factory ?? throw new InvalidOperationException("La plataforma no ha arrancado.");

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:EnergyDatabase", database.GetConnectionString());
            builder.UseSetting("Seed:DataDirectory", ChallengeDataDirectory());
            builder.UseSetting("Auth:Mode", "Demo");
            builder.UseSetting("Anthropic:Enabled", "false");
            builder.UseSetting("Analysis:Pacing:MinimumStageDuration", "00:00:00");
        });
        Factory.CreateClient().Dispose();
    }

    public async Task DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await database.DisposeAsync();
    }

    public HttpClient AnonymousClient() => Factory.CreateClient();

    public async Task<HttpClient> SignedInClientAsync()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "demo@vatio.app", password = "demo1234" });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<JsonNode>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!["token"]!.GetValue<string>());
        return client;
    }

    public async Task<JsonNode> CompletedAnalysisAsync()
    {
        await analysisLock.WaitAsync();
        try
        {
            return completedAnalysis ??= await RunAnalysisAsync();
        }
        finally
        {
            analysisLock.Release();
        }
    }

    private async Task<JsonNode> RunAnalysisAsync()
    {
        using var client = await SignedInClientAsync();
        var started = await client.PostAsync("/api/ai/analyze", null);
        started.EnsureSuccessStatusCode();
        var runId = (await started.Content.ReadFromJsonAsync<JsonNode>())!["id"]!.GetValue<Guid>();

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            var run = (await client.GetFromJsonAsync<JsonNode>($"/api/ai/analysis/{runId}"))!;
            if (run["status"]!.GetValue<string>() is "COMPLETED" or "FAILED")
            {
                return run;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("El análisis no terminó en 30 segundos.");
    }

    private static string ChallengeDataDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "data", "readings.csv")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new DirectoryNotFoundException("No se encontró la carpeta data con los CSV del reto.")
            : Path.Combine(directory.FullName, "data");
    }
}

[CollectionDefinition(Name)]
public sealed class PlatformCollection : ICollectionFixture<PlatformFixture>
{
    public const string Name = "Plataforma";
}
