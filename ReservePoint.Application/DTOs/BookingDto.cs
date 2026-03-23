namespace ReservePoint.Application.DTOs;

public record BookingDto(
    Guid Id,
    Guid ResourceId,
    string IdentityId,
    Guid OrganizationId,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    DateTime CreatedAt,
    BookingPolicySnapshotDto AppliedPolicy
);

public record BookingPolicySnapshotDto(
    int MaxDurationHours,
    int MaxBookingsPerUser
);