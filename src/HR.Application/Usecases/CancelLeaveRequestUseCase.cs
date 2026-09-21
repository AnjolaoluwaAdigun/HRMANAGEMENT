using HR.Application.Exceptions;
using HR.Application.Interfaces;
using HR.Domain.Entities;

namespace HR.Application.UseCases;

public class CancelLeaveRequestUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelLeaveRequestUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid leaveRequestId, Guid requestingEmployeeId)
    {
        var request = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        // An employee can only cancel their own requests.
        if (request.EmployeeId != requestingEmployeeId)
            throw new ForbiddenException("You can only cancel your own leave requests.");

        // Domain's Cancel() already throws InvalidOperationException for non-Pending
        // requests (e.g. already Approved) — that satisfies US4's rejection rule.
        request.Cancel();

        await _unitOfWork.SaveChangesAsync();
    }
}