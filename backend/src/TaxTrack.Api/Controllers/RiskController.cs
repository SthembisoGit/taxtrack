using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Api.Contracts;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/risk")]
public sealed class RiskController(IRiskService riskService) : ControllerBase
{
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalyzeAcceptedResponse>> Analyze([FromBody] AnalyzeRiskApiRequest request, CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Idempotency-Key header is required.");
        }

        var userId = User.GetUserId();
        var response = await riskService.AnalyzeAsync(
            new AnalyzeRiskCommand
            {
                UserId = userId,
                CompanyId = request.CompanyId,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                IdempotencyKey = idempotencyKey.ToString()
            },
            HttpContext.GetCorrelationId(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return Accepted(response);
    }

    [HttpGet("analyze/{analysisId:guid}/status")]
    public async Task<ActionResult<RiskAnalysisJobStatusResponse>> AnalysisStatus(Guid analysisId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await riskService.GetStatusAsync(userId, analysisId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("{companyId:guid}")]
    public async Task<ActionResult<RiskResultResponse>> Latest(Guid companyId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await riskService.GetLatestResultAsync(userId, companyId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
