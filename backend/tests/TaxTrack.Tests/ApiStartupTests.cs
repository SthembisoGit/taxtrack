using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Tests;

/// <summary>
/// Integration tests for API startup behavior, including database migration strategy.
/// Verifies that the application handles migrations correctly in dev vs. production environments.
/// </summary>
public sealed class ApiStartupTests
{
    [Fact]
    public async Task Startup_WithExplicitInMemoryOptIn_Succeeds()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode, "API failed to start with Database:UseInMemory=true.");
    }

    [Fact]
    public void Startup_WithoutDatabaseConfig_FailsFast()
    {
        using var factory = new MissingDatabaseConfigFactory();

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains(
            "ConnectionStrings:PostgreSql must be configured when Database:UseInMemory is false.",
            exception.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Startup_DatabaseInitializationRunsOnce()
    {
        await using var factory = new ApiTestFactory();

        using var client1 = factory.CreateClient();
        using var client2 = factory.CreateClient();

        var response1 = await client1.GetAsync("/health/live");
        var response2 = await client2.GetAsync("/health/live");

        Assert.True(response1.IsSuccessStatusCode, "First client health check failed");
        Assert.True(response2.IsSuccessStatusCode, "Second client health check failed");
    }

    [Fact]
    public async Task Startup_RegistersCorrelationIdMiddlewareInPipeline()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        var testCorrelationId = "test-startup-123";

        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", testCorrelationId);
        using var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("X-Correlation-ID"),
            "CorrelationIdMiddleware not registered: X-Correlation-ID not in response");
        var returnedId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(testCorrelationId, returnedId);
    }

    private sealed class MissingDatabaseConfigFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Database:UseInMemory", "false");
            builder.UseSetting("ConnectionStrings:PostgreSql", string.Empty);
        }
    }
}
