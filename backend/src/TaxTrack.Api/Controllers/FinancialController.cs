using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Api.Contracts;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/financial")]
public sealed class FinancialController(IUploadService uploadService) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<UploadAcceptedResponse>> Upload([FromForm] UploadApiRequest request, CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Idempotency-Key header is required.");
        }

        if (!TryParseDatasetType(request.DatasetType, out var datasetType))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Invalid datasetType.");
        }

        var userId = User.GetUserId();
        await using var stream = request.File.OpenReadStream();

        var response = await uploadService.UploadAsync(
            new UploadCommand
            {
                UserId = userId,
                CompanyId = request.CompanyId,
                DatasetType = datasetType,
                FileName = request.File.FileName,
                Content = stream,
                IdempotencyKey = idempotencyKey.ToString()
            },
            HttpContext.GetCorrelationId(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return Accepted(response);
    }

    [HttpGet("upload/{uploadId:guid}/status")]
    public async Task<ActionResult<UploadStatusResponse>> UploadStatus(Guid uploadId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await uploadService.GetUploadStatusAsync(userId, uploadId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    private static bool TryParseDatasetType(string raw, out DatasetType datasetType)
    {
        datasetType = raw.Trim().ToLowerInvariant() switch
        {
            "company" => DatasetType.Company,
            "transactions" => DatasetType.Transactions,
            "payroll" => DatasetType.Payroll,
            "vat_submissions" => DatasetType.VatSubmissions,
            _ => 0
        };

        return datasetType != 0;
    }
}
