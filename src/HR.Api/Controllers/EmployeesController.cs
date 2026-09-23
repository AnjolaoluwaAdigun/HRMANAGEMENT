using System.Security.Claims;
using HR.Application.DTOs;
using HR.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly OnboardEmployeeUseCase _onboardEmployeeUseCase;

    public EmployeesController(OnboardEmployeeUseCase onboardEmployeeUseCase) =>
        _onboardEmployeeUseCase = onboardEmployeeUseCase;

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

    // Helper used by other controllers to read the caller's identity from the JWT.
    public static Guid GetCurrentEmployeeId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}