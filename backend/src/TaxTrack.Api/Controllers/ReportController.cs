using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaxTrack.Api.Common;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/report")]
public sealed class ReportController(IReportService reportService) : ControllerBase
{
    private static readonly JsonSerializerOptions DownloadJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [HttpGet("{companyId:guid}")]
    public async Task<ActionResult<ReportResponse>> Generate(Guid companyId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await reportService.GenerateAsync(
            userId,
            companyId,
            HttpContext.GetCorrelationId(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("{companyId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid companyId,
        [FromQuery] Guid reportId,
        [FromQuery] string format,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await reportService.DownloadJsonAsync(
            userId,
            companyId,
            reportId,
            format,
            token,
            HttpContext.GetCorrelationId(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        var content = JsonSerializer.Serialize(response.Payload, DownloadJsonOptions);
        return File(System.Text.Encoding.UTF8.GetBytes(content), response.ContentType, response.FileName);
    }
}
