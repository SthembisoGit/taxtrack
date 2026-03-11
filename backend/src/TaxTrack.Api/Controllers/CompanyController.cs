using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxTrack.Api.Common;
using TaxTrack.Api.Contracts;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/company")]
public sealed class CompanyController(ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompanyResponse>>> List(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await companyService.ListCompaniesAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyApiRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await companyService.CreateCompanyAsync(
            userId,
            new CreateCompanyRequest(request.Name, request.RegistrationNumber, request.Industry, request.TaxReference),
            cancellationToken);

        return Created($"/api/company/{response.Id}", response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await companyService.GetCompanyAsync(userId, id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
