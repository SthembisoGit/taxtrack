using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/audit")]
public sealed class AuditController(IAuditQueryService auditQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AuditLogEventResponse>>> List(
        [FromQuery] Guid? companyId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var response = await auditQueryService.ListAsync(userId, companyId, limit, cancellationToken);
        return Ok(response);
    }
}
