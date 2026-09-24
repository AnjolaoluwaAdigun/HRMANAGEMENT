using System.Security.Claims;
using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.UseCases;
using HR.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly OnboardEmployeeUseCase _onboardEmployeeUseCase;
     private readonly UpdateEmployeeUseCase _updateEmployeeUseCase;
    private readonly DeactivateEmployeeUseCase _deactivateEmployeeUseCase;

    public EmployeesController(OnboardEmployeeUseCase onboardEmployeeUseCase, UpdateEmployeeUseCase updateEmployeeUseCase,
        DeactivateEmployeeUseCase deactivateEmployeeUseCase)
        {
        _onboardEmployeeUseCase = onboardEmployeeUseCase;
        _updateEmployeeUseCase = updateEmployeeUseCase;
        _deactivateEmployeeUseCase = deactivateEmployeeUseCase;
        }

    // FR1, US6 — HR Admin only.
    [HttpPost]
    [Authorize(Roles = "HRAdmin")]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto)
    {
        try
        {
            var result = await _onboardEmployeeUseCase.ExecuteAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = result.EmployeeId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // FR1, US-FE2 — HR Admin only.
    [HttpPut("{id}")]
    [Authorize(Roles = "HRAdmin")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, UpdateEmployeeDto dto)
    {
        try
        {
            var result = await _updateEmployeeUseCase.ExecuteAsync(id, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // FR1, US-FE3 — HR Admin only.
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "HRAdmin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _deactivateEmployeeUseCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Helper used by other controllers to read the caller's identity from the JWT.
    public static Guid GetCurrentEmployeeId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}