using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Application.Exceptions;

namespace TaxTrack.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity, "Validation Failed", ex.Message, ex.Issues);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled server exception.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        object? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        var payload = errors is null
            ? JsonSerializer.Serialize(problem)
            : JsonSerializer.Serialize(new
            {
                problem.Type,
                problem.Title,
                problem.Status,
                problem.Detail,
                problem.Instance,
                errors
            });

        await context.Response.WriteAsync(payload);
    }
}
