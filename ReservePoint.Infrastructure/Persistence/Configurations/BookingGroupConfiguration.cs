using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservePoint.Domain.Entities;

namespace ReservePoint.Infrastructure.Persistence.Configurations;

public class BookingGroupConfiguration : IEntityTypeConfiguration<BookingGroup>
{
    public void Configure(EntityTypeBuilder<BookingGroup> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.IdentityId).IsRequired().HasMaxLength(256);
        builder.Property(b => b.OrganizationId).IsRequired();
        builder.Property(b => b.StartTime).IsRequired();
        builder.Property(b => b.EndTime).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.OwnsOne(b => b.AppliedPolicy, policy =>
        {
            policy.Property(p => p.MaxDurationHours)
                .HasColumnName("Policy_MaxDurationHours");
            policy.Property(p => p.MaxBookingsPerUser)
                .HasColumnName("Policy_MaxBookingsPerUser");
            policy.Property(p => p.AllowedTimeFrom)
                .HasColumnName("Policy_AllowedTimeFrom")
                .HasConversion(
                    t => t.ToTimeSpan(),
                    t => TimeOnly.FromTimeSpan(t));
            policy.Property(p => p.AllowedTimeTo)
                .HasColumnName("Policy_AllowedTimeTo")
                .HasConversion(
                    t => t.ToTimeSpan(),
                    t => TimeOnly.FromTimeSpan(t));
        });

        builder.HasMany(b => b.Bookings)
            .WithOne()
            .HasForeignKey(b => b.BookingGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
