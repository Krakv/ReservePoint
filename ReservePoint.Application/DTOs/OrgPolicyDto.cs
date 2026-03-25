namespace ReservePoint.Application.DTOs;

public record OrgPolicyDto(
     int ID,
     Guid OrganizationID,
     int MaxBookingDurationMin,
     int BookingWindowDays,
     int MaxActiveBookingsPerUser,
     DateTime CreatedAt,
     DateTime UpdatedAt
);