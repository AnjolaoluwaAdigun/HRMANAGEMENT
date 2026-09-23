using HR.Application.DTOs;
using HR.Application.Exceptions;
using HR.Application.UseCases;
using HR.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.Api.Controllers;

[ApiController]
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly SubmitLeaveRequestUseCase _submitUseCase;
    private readonly ApproveLeaveRequestUseCase _approveUseCase;
    private readonly RejectLeaveRequestUseCase _rejectUseCase;
    private readonly CancelLeaveRequestUseCase _cancelUseCase;
    private readonly GetTeamLeaveCalendarUseCase _teamCalendarUseCase;
    private readonly GetLeaveRequestHistoryUseCase _historyUseCase;

    public LeaveRequestsController(
        SubmitLeaveRequestUseCase submitUseCase,
        ApproveLeaveRequestUseCase approveUseCase,
        RejectLeaveRequestUseCase rejectUseCase,
        CancelLeaveRequestUseCase cancelUseCase,
        GetTeamLeaveCalendarUseCase teamCalendarUseCase,
         GetLeaveRequestHistoryUseCase historyUseCase)
    {
        _submitUseCase = submitUseCase;
        _approveUseCase = approveUseCase;
        _rejectUseCase = rejectUseCase;
        _cancelUseCase = cancelUseCase;
        _teamCalendarUseCase = teamCalendarUseCase;
         _historyUseCase = historyUseCase;
    }

    // US1, FR4-FR6
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Submit(SubmitLeaveRequestDto dto)
    {
        var callerId = EmployeesController.GetCurrentEmployeeId(User);
        if (dto.EmployeeId != callerId)
            return Forbid();

        try
        {
            var result = await _submitUseCase.ExecuteAsync(dto);
            return CreatedAtAction(nameof(Submit), new { id = result.LeaveRequestId }, result);
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (OverlappingLeaveRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // US2, FR9-FR10 — Manager only.
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager,HRAdmin")]
    public async Task<ActionResult<LeaveRequestDto>> Approve(Guid id, [FromBody] string? comment)
    {
        var managerId = EmployeesController.GetCurrentEmployeeId(User);

        try
        {
            var result = await _approveUseCase.ExecuteAsync(id, managerId, comment);
            return Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // US2 — Manager only.
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Manager,HRAdmin")]
    public async Task<ActionResult<LeaveRequestDto>> Reject(Guid id, [FromBody] string? comment)
    {
        var managerId = EmployeesController.GetCurrentEmployeeId(User);

        try
        {
            var result = await _rejectUseCase.ExecuteAsync(id, managerId, comment);
            return Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // US4, FR7
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var callerId = EmployeesController.GetCurrentEmployeeId(User);

        try
        {
            await _cancelUseCase.ExecuteAsync(id, callerId);
            return NoContent();
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // US5, FR14 — Manager only.
    [HttpGet("team-calendar")]
    [Authorize(Roles = "Manager,HRAdmin")]
    public async Task<ActionResult<List<LeaveRequestDto>>> TeamCalendar(
        [FromQuery] DateOnly rangeStart, [FromQuery] DateOnly rangeEnd)
    {
        var managerId = EmployeesController.GetCurrentEmployeeId(User);
        var result = await _teamCalendarUseCase.ExecuteAsync(managerId, rangeStart, rangeEnd);
        return Ok(result);
    }

        // FR13, US-FE9 — employees can only ever see their own (enforced via JWT identity, no route param).
    [HttpGet("my-history")]
    public async Task<ActionResult<List<LeaveRequestDto>>> MyHistory()
    {
        var callerId = EmployeesController.GetCurrentEmployeeId(User);
        var result = await _historyUseCase.ExecuteAsync(callerId);
        return Ok(result);
    }
}