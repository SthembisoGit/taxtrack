using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxTrack.Api.Common;
using TaxTrack.Infrastructure;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;
using TaxTrack.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var defaultDevOrigins = new[]
{
    "http://localhost:5173",
    "http://127.0.0.1:5173",
    "https://localhost:5173"
};

if (builder.Environment.IsDevelopment() && corsOrigins.Length == 0)
{
    corsOrigins = defaultDevOrigins;
}

if (!builder.Environment.IsDevelopment() && corsOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must be configured for non-development environments.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException("JWT signing key must be at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("global", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 100;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 10;
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();

// Database initialization: Apply migrations or validate schema
// Production: Fail fast if migrations are pending (prevent silent data inconsistencies)
// Development: Auto-apply migrations for smooth iteration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaxTrackDbContext>();
    if (db.Database.IsRelational())
    {
        if (app.Environment.IsProduction())
        {
            // PRODUCTION: Validate that migrations have been applied via explicit release process
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                throw new InvalidOperationException("Pending database migrations detected. Apply migrations before starting the API.");
            }
        }
        else
        {
            // DEVELOPMENT: Auto-apply pending migrations to enable rapid iteration
            await db.Database.MigrateAsync();
        }
    }
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () =>
{
    var response = new HealthStatusResponse(
        "TaxTrack.Api",
        "Healthy",
        DateTime.UtcNow,
        [new HealthCheckEntryResponse("api", "Healthy", "Process is running.")]);

    return Results.Ok(response);
});

app.MapGet("/health/ready", async (IServiceScopeFactory scopeFactory) =>
{
    await using var scope = scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TaxTrackDbContext>();

    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            var unavailable = new HealthStatusResponse(
                "TaxTrack.Api",
                "Unhealthy",
                DateTime.UtcNow,
                [new HealthCheckEntryResponse("database", "Unhealthy", "Database connectivity check failed.")]);

            return Results.Json(unavailable, statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var healthy = new HealthStatusResponse(
            "TaxTrack.Api",
            "Healthy",
            DateTime.UtcNow,
            [new HealthCheckEntryResponse("database", "Healthy", "Database connectivity verified.")]);

        return Results.Ok(healthy);
    }
    catch (Exception ex)
    {
        var failure = new HealthStatusResponse(
            "TaxTrack.Api",
            "Unhealthy",
            DateTime.UtcNow,
            [new HealthCheckEntryResponse("database", "Unhealthy", ex.Message)]);

        return Results.Json(failure, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapControllers().RequireRateLimiting("global");

app.Run();

public partial class Program;
