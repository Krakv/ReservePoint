namespace ReservePoint.Application.DTOs;

public record BusySlotDto(
    DateTime StartTime,
    DateTime EndTime
);
