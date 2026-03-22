using ReservePoint.Domain.Enums;

namespace ReservePoint.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid ResourceId { get; private set; }
    public string IdentityId { get; private set; } = null!;
    public Guid OrganizationId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public BookingPolicySnapshot AppliedPolicy { get; private set; } = null!;

    private Booking() { }

    public static Booking Create(
        Guid resourceId,
        string identityId,
        Guid organizationId,
        DateTime startTime,
        DateTime endTime,
        BookingPolicySnapshot appliedPolicy)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            IdentityId = identityId,
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