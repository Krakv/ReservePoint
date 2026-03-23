namespace ReservePoint.Domain.Entities;

public class BookingPolicySnapshot
{
    public int MaxDurationHours { get; set; }
    public int MaxBookingsPerUser { get; set; }

    private BookingPolicySnapshot() { }

    public static BookingPolicySnapshot Create(
        int maxDurationHours,
        int maxBookingsPerUser)
    {
        return new BookingPolicySnapshot
        {
            MaxDurationHours = maxDurationHours,
            MaxBookingsPerUser = maxBookingsPerUser
        };
    }
}
