using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"taxtrack-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TaxTrackDbContext>>();
            services.RemoveAll<TaxTrackDbContext>();
            services.AddDbContext<TaxTrackDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
