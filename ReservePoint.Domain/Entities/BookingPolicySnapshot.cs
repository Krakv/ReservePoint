namespace ReservePoint.Domain.Entities;

public class BookingPolicySnapshot
{
    public int MaxDurationHours { get; set; }
    public int MaxBookingsPerUser { get; set; }
    public TimeOnly AllowedTimeFrom { get; set; }
    public TimeOnly AllowedTimeTo { get; set; }

    private BookingPolicySnapshot() { }

    public static BookingPolicySnapshot Create(
        int maxDurationHours,
        int maxBookingsPerUser,
        TimeOnly allowedTimeFrom,
        TimeOnly allowedTimeTo)
    {
        return new BookingPolicySnapshot
        {
            MaxDurationHours = maxDurationHours,
            MaxBookingsPerUser = maxBookingsPerUser,
            AllowedTimeFrom = allowedTimeFrom,
            AllowedTimeTo = allowedTimeTo
        };
    }
}
