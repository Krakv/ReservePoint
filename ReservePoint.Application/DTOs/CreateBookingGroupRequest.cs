namespace ReservePoint.Application.DTOs;

public record CreateBookingGroupRequest(
    Guid OrganizationId,
    IEnumerable<Guid> ResourceIds,
    DateTime StartTime,
    DateTime EndTime
);
