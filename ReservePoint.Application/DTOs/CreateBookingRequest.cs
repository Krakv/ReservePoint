namespace ReservePoint.Application.DTOs;

public record CreateBookingRequest(
    Guid OrganizationId,
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
);