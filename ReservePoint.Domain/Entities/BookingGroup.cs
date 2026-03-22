using ReservePoint.Domain.Enums;

namespace ReservePoint.Domain.Entities;

public class BookingGroup
{
    public Guid Id { get; private set; }
    public string IdentityId { get; private set; } = null!;
    public Guid OrganizationId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public BookingGroupStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public BookingPolicySnapshot AppliedPolicy { get; private set; } = null!;
    public ICollection<Booking> Bookings { get; private set; } = [];

    private BookingGroup() { }

    public static BookingGroup Create(
        string identityId,
        Guid organizationId,
        DateTime startTime,
        DateTime endTime,
        BookingPolicySnapshot appliedPolicy,
        IEnumerable<Guid> resourceIds)
    {
        var group = new BookingGroup
        {
            Id = Guid.NewGuid(),
            IdentityId = identityId,
            OrganizationId = organizationId,
            StartTime = startTime,
            EndTime = endTime,
            Status = BookingGroupStatus.Active,
            CreatedAt = DateTime.UtcNow,
            AppliedPolicy = appliedPolicy,
        };

        foreach (var resourceId in resourceIds)
            group.Bookings.Add(Booking.Create(resourceId, group.Id));

        return group;
    }

    public void Cancel()
    {
        if (Status == BookingGroupStatus.Cancelled)
            throw new InvalidOperationException("Бронирование уже отменено");

        Status = BookingGroupStatus.Cancelled;

        foreach (var booking in Bookings)
            booking.Cancel();
    }
}
