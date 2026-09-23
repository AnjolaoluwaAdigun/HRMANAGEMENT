using HR.Application.DTOs;
using HR.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/leave-balances")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly GetLeaveBalanceUseCase _getLeaveBalanceUseCase;

    public LeaveBalancesController(GetLeaveBalanceUseCase getLeaveBalanceUseCase) =>
        _getLeaveBalanceUseCase = getLeaveBalanceUseCase;

    // US3, FR12 — employees can only ever see their own (enforced by using the JWT identity, not a route param).
    [HttpGet]
    public async Task<ActionResult<List<LeaveBalanceDto>>> GetMyBalance([FromQuery] int year)
    {
        var callerId = EmployeesController.GetCurrentEmployeeId(User);
        var result = await _getLeaveBalanceUseCase.ExecuteAsync(callerId, year);
        return Ok(result);
    }
}