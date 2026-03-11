using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Api.Contracts;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/privacy/data-requests")]
public sealed class PrivacyController(IPrivacyService privacyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DataSubjectRequestResponse>>> List(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await privacyService.ListRequestsAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<DataSubjectRequestResponse>> Create([FromBody] CreateDataRequestApiRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await privacyService.CreateRequestAsync(
            new CreateDataSubjectRequestCommand
            {
                RequesterUserId = userId,
                CompanyId = request.CompanyId,
                RequestType = request.RequestType,
                Reason = request.Reason
            },
            HttpContext.GetCorrelationId(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return Accepted(response);
    }

    [HttpGet("{requestId:guid}")]
    public async Task<ActionResult<DataSubjectRequestResponse>> Get(Guid requestId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await privacyService.GetRequestAsync(userId, requestId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
