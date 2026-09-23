using HR.Application.DTOs;
using HR.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/leave-types")]
[Authorize]
public class LeaveTypesController : ControllerBase
{
    private readonly CreateLeaveTypeUseCase _createUseCase;
    private readonly GetLeaveTypesUseCase _getUseCase;
    private readonly AllocateYearlyBalancesUseCase _allocateYearlyUseCase;

    public LeaveTypesController(CreateLeaveTypeUseCase createUseCase, GetLeaveTypesUseCase getUseCase, AllocateYearlyBalancesUseCase allocateYearlyUseCase)
    {
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _allocateYearlyUseCase = allocateYearlyUseCase;
    }

    // FR2 — HR Admin only.
    [HttpPost]
    [Authorize(Roles = "HRAdmin")]
    public async Task<ActionResult<LeaveTypeDto>> Create(CreateLeaveTypeDto dto)
    {
        var result = await _createUseCase.ExecuteAsync(dto);
        return CreatedAtAction(nameof(Create), new { id = result.LeaveTypeId }, result);
    }

    // Any authenticated user can view available leave types (needed for the submit-request form).
    [HttpGet]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetAll()
    {
        var result = await _getUseCase.ExecuteAsync();
        return Ok(result);
    }
    // FR3 — HR Admin triggers the yearly balance allocation batch job. In a real
    // production system this would likely run on a schedule instead of on-demand,
    // but exposing it as an endpoint satisfies the functional requirement and
    // makes it demonstrable/testable.
    [HttpPost("allocate-yearly-balances")]
    [Authorize(Roles = "HRAdmin")]
    public async Task<IActionResult> AllocateYearlyBalances(AllocateYearlyBalancesDto dto)
    {
        await _allocateYearlyUseCase.ExecuteAsync(dto.Year);
        return Ok(new { message = $"Yearly balances allocated for {dto.Year}." });
    }
}