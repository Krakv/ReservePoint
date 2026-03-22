using ReservePoint.Domain.Enums;

namespace ReservePoint.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid BookingGroupId { get; private set; }
    public Guid ResourceId { get; private set; }
    public BookingStatus Status { get; private set; }

    private Booking() { }

    internal static Booking Create(Guid resourceId, Guid bookingGroupId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            BookingGroupId = bookingGroupId,
            Status = BookingStatus.Active,
        };
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Бронирование уже отменено");

        Status = BookingStatus.Cancelled;
    }
}
