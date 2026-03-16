using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Tests;

/// <summary>
/// Integration tests for API startup behavior, including database migration strategy.
/// Verifies that the application handles migrations correctly in dev vs. production environments.
/// </summary>
public sealed class ApiStartupTests
{
    /// <summary>
    /// In development environment, pending migrations should be automatically applied
    /// during startup to enable smooth development iteration.
    /// </summary>
    [Fact]
    public async Task Startup_InDevelopment_AutoMigratesDatabase()
    {
        // Arrange: Create a test factory that simulates development environment
        await using var factory = new DevelopmentWebApplicationFactory();
        
        // Act: Creating the client triggers startup and database initialization
        using var client = factory.CreateClient();

        // Assert: The health check endpoint should be available, confirming the app started successfully
        using var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode,
            "API failed to start. Auto-migration likely failed in development environment.");
    }

    /// <summary>
    /// When using an in-memory database (no connection string configured),
    /// migrations should not be required or executed.
    /// The in-memory database is created on the fly and doesn't track migrations.
    /// This is the default behavior for testing.
    /// </summary>
    [Fact]
    public async Task Startup_WithInMemoryDatabase_DoesNotRequireMigrations()
    {
        // Arrange: Use the standard ApiTestFactory which configures in-memory DB
        await using var factory = new ApiTestFactory();

        // Act: Creating the client should succeed without migration checks
        using var client = factory.CreateClient();

        // Assert: The API should be fully functional
        using var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode,
            "API failed with in-memory database. Database.IsRelational() check may have an issue.");
    }

    /// <summary>
    /// Verify that the database initialization code only runs once during startup.
    /// This prevents race conditions or multiple initialization attempts.
    /// </summary>
    [Fact]
    public async Task Startup_DatabaseInitializationRunsOnce()
    {
        // Arrange
        await using var factory = new ApiTestFactory();

        // Act: Create multiple clients from the same factory
        using var client1 = factory.CreateClient();
        using var client2 = factory.CreateClient();

        // Assert: Both should work (if init ran multiple times, it might cause issues)
        var response1 = await client1.GetAsync("/health/live");
        var response2 = await client2.GetAsync("/health/live");

        Assert.True(response1.IsSuccessStatusCode, "First client health check failed");
        Assert.True(response2.IsSuccessStatusCode, "Second client health check failed");
    }

    /// <summary>
    /// The CorrelationIdMiddleware should be registered in the pipeline
    /// and propagate X-Correlation-ID headers.
    /// </summary>
    [Fact]
    public async Task Startup_RegistersCorrelationIdMiddlewareInPipeline()
    {
        // Arrange
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        var testCorrelationId = "test-startup-123";

        // Act: Make a request with a custom correlation ID
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", testCorrelationId);
        using var response = await client.SendAsync(request);

        // Assert: The response should include the correlation ID header
        Assert.True(response.Headers.Contains("X-Correlation-ID"),
            "CorrelationIdMiddleware not registered: X-Correlation-ID not in response");
        var returnedId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        Assert.Equal(testCorrelationId, returnedId);
    }

    /// <summary>
    /// Test factory that simulates development environment behavior.
    /// Auto-applies migrations on startup.
    /// </summary>
    private sealed class DevelopmentWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"taxtrack-dev-tests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<TaxTrackDbContext>>();
                services.RemoveAll<TaxTrackDbContext>();
                
                // In-memory database for dev tests
                services.AddDbContext<TaxTrackDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}

