namespace ReservePoint.Application.DTOs;

public record OrgPolicyDto(
    Guid OrganizationId,
    int MaxBookingsPerUser,
    TimeOnly AllowedTimeFrom,
    TimeOnly AllowedTimeTo,
    int BookingHorizonDays
);
