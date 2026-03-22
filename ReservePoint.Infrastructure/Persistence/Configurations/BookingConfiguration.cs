using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservePoint.Domain.Entities;

namespace ReservePoint.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ResourceId).IsRequired();
        builder.Property(b => b.BookingGroupId).IsRequired();
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}
