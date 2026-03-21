namespace ReservePoint.Application.DTOs;

public record CreateBookingRequest(
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
);
