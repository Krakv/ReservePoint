namespace ReservePoint.Application.DTOs;

public record BookingGroupDto(
    Guid Id,
    string IdentityId,
    Guid OrganizationId,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    DateTime CreatedAt,
    IEnumerable<BookingItemDto> Bookings,
    BookingPolicySnapshotDto AppliedPolicy
);

public record BookingItemDto(
    Guid Id,
    Guid ResourceId,
    string Status
);
