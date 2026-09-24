namespace HR.Application.DTOs;

public record EmployeeOnLeaveDto(
    Guid EmployeeId,
    string Name,
    string Department,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate);

public record DepartmentUsageDto(
    string Department,
    int EmployeeCount,
    decimal TotalApprovedDaysThisYear);

public record CompanyLeaveStatsDto(
    int TotalEmployees,
    int EmployeesOnLeaveToday,
    List<EmployeeOnLeaveDto> CurrentlyOnLeave,
    List<DepartmentUsageDto> DepartmentUsage);