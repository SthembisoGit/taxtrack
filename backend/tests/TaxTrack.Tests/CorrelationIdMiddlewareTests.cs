using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TaxTrack.Api.Middleware;

namespace TaxTrack.Tests;

/// <summary>
/// Unit tests for CorrelationIdMiddleware to verify request tracing functionality.
/// </summary>
public sealed class CorrelationIdMiddlewareTests
{
    /// <summary>
    /// When X-Correlation-ID header is provided, the middleware should use it
    /// and propagate it to the response headers.
    /// </summary>
    [Theory]
    [InlineData("123e4567-e89b-12d3-a456-426614174000")]
    [InlineData("request-trace-12345")]
    [InlineData("production-incident-2026-03-16")]
    public async Task Invoke_WithCorrelationIdHeader_UsesSuppliedIdAndPropagatesToResponse(string correlationId)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = correlationId;

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled, "The next middleware was not invoked");
        Assert.True(context.Response.Headers.TryGetValue("X-Correlation-ID", out var responseValue),
            "X-Correlation-ID was not added to response headers");
        Assert.Equal(correlationId, responseValue.ToString());
    }

    /// <summary>
    /// When X-Correlation-ID header is missing, the middleware should use
    /// the TraceIdentifier and propagate it to the response headers.
    /// </summary>
    [Fact]
    public async Task Invoke_WithoutCorrelationIdHeader_GeneratesFromTraceIdentifierAndPropagatesToResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        var traceId = context.TraceIdentifier;

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled, "The next middleware was not invoked");
        Assert.True(context.Response.Headers.TryGetValue("X-Correlation-ID", out var responseValue),
            "X-Correlation-ID was not added to response headers");
        Assert.Equal(traceId, responseValue.ToString());
    }

    /// <summary>
    /// When X-Correlation-ID header is empty or whitespace, the middleware should
    /// fall back to TraceIdentifier (equivalent to missing header).
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task Invoke_WithEmptyOrWhitespaceCorrelationIdHeader_UsesTraceIdentifier(string emptyValue)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new CorrelationIdMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = emptyValue;
        var traceId = context.TraceIdentifier;

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(context.Response.Headers.TryGetValue("X-Correlation-ID", out var responseValue),
            "X-Correlation-ID was not added to response headers");
        Assert.Equal(traceId, responseValue.ToString());
    }

    /// <summary>
    /// The middleware should establish a logging scope with the correlation ID
    /// so that all logs within the request include the correlation context.
    /// </summary>
    [Fact]
    public async Task Invoke_EstablishesLoggingScope()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        var capturedScopes = new List<(Dictionary<string, object> state, IDisposable disposable)>();

        mockLogger
            .Setup(x => x.BeginScope(It.IsAny<It.IsAnyType>()))
            .Returns(new Mock<IDisposable>().Object)
            .Callback((object state) =>
            {
                if (state is Dictionary<string, object> dict)
                {
                    capturedScopes.Add((new Dictionary<string, object>(dict), null!));
                }
            });

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        var correlationId = "test-correlation-12345";
        context.Request.Headers["X-Correlation-ID"] = correlationId;

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.NotEmpty(capturedScopes);
        Assert.Contains(
            capturedScopes,
            scope => scope.state.TryGetValue("CorrelationId", out var value) &&
                     string.Equals(value?.ToString(), correlationId, StringComparison.Ordinal));
    }

    /// <summary>
    /// The middleware must continue the request pipeline by invoking the next delegate.
    /// </summary>
    [Fact]
    public async Task Invoke_CallsNextDelegate()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        var nextInvoked = false;
        RequestDelegate next = async (ctx) =>
        {
            nextInvoked = true;
            // Simulate downstream middleware/endpoint
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            await Task.Delay(10); // Simulate async work
        };

        var middleware = new CorrelationIdMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextInvoked, "The next delegate was not invoked");
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}
