using ReservePoint.Domain.Enums;

namespace ReservePoint.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public BookingPolicySnapshot AppliedPolicy { get; set; } = null!;

    private Booking() { }

    public static Booking Create(
        Guid resourceId,
        Guid userId,
        Guid organizationId,
        DateTime startTime,
        DateTime endTime,
        BookingPolicySnapshot appliedPolicy)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            UserId = userId,
            OrganizationId = organizationId,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow,
            AppliedPolicy = appliedPolicy
        };
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Бронирование уже отменено");

        Status = BookingStatus.Cancelled;
    }
}
