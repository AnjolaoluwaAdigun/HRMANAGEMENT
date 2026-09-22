using HR.Application.Exceptions;
using HR.Application.UseCases;
using HR.Domain.Entities;
using HR.Domain.Enums;
using Moq;
using Xunit;

namespace HR.Application.Tests;

public class CancelLeaveRequestUseCaseTests
{
    [Fact]
    public async Task Execute_OwnPendingRequest_CancelsSuccessfully()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employeeId = Guid.NewGuid();
        var request = new LeaveRequest(
            employeeId, Guid.NewGuid(),
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), 5, "Trip");

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);

        var useCase = new CancelLeaveRequestUseCase(builder.Build());
        await useCase.ExecuteAsync(request.LeaveRequestId, employeeId);

        Assert.Equal(RequestStatus.Cancelled, request.Status);
    }

    [Fact]
    public async Task Execute_SomeoneElsesRequest_ThrowsForbiddenException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var request = new LeaveRequest(
            ownerId, Guid.NewGuid(),
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), 5, "Trip");

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);

        var useCase = new CancelLeaveRequestUseCase(builder.Build());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            useCase.ExecuteAsync(request.LeaveRequestId, strangerId));

        Assert.Equal(RequestStatus.Pending, request.Status); // untouched
    }

    [Fact]
    public async Task Execute_AlreadyApprovedRequest_ThrowsInvalidOperationException()
    {
        var builder = new TestUnitOfWorkBuilder();
        var employeeId = Guid.NewGuid();
        var request = new LeaveRequest(
            employeeId, Guid.NewGuid(),
            new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 9), 5, "Trip");
        request.Approve(Guid.NewGuid());

        builder.LeaveRequests.Setup(r => r.GetByIdAsync(request.LeaveRequestId)).ReturnsAsync(request);

        var useCase = new CancelLeaveRequestUseCase(builder.Build());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(request.LeaveRequestId, employeeId));
    }
}