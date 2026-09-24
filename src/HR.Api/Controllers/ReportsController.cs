using HR.Application.DTOs;
using HR.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "HRAdmin")]
public class ReportsController : ControllerBase
{
    private readonly GetCompanyLeaveStatsUseCase _companyStatsUseCase;

    public ReportsController(GetCompanyLeaveStatsUseCase companyStatsUseCase) =>
        _companyStatsUseCase = companyStatsUseCase;

    // FR15, US-FE11 — HR Admin only.
    [HttpGet("company-stats")]
    public async Task<ActionResult<CompanyLeaveStatsDto>> CompanyStats()
    {
        var result = await _companyStatsUseCase.ExecuteAsync();
        return Ok(result);
    }
}