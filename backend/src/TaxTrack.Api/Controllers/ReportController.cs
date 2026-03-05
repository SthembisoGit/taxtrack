using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/report")]
public sealed class ReportController(IReportService reportService) : ControllerBase
{
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
}
