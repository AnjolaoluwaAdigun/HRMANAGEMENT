using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(r => r.LeaveRequestId);

        builder.Property(r => r.RequestedDays).HasColumnType("decimal(5,2)");
        builder.Property(r => r.Reason).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LeaveType>().WithMany().HasForeignKey(r => r.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.ApprovedBy).OnDelete(DeleteBehavior.Restrict);
    }
}