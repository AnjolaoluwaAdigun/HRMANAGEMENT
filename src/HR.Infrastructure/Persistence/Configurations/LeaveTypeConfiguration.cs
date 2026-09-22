using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.HasKey(t => t.LeaveTypeId);
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.Property(t => t.DefaultDaysPerYear).IsRequired();
    }
}