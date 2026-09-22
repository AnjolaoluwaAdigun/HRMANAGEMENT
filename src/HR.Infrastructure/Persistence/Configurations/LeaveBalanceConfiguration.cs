using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.HasKey(b => b.LeaveBalanceId);

        builder.Property(b => b.RemainingDays).HasColumnType("decimal(5,2)");

        builder.HasIndex(b => new { b.EmployeeId, b.LeaveTypeId, b.Year }).IsUnique();

        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.HasOne<Employee>().WithMany().HasForeignKey(b => b.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LeaveType>().WithMany().HasForeignKey(b => b.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}