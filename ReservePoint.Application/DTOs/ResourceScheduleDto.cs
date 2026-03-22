namespace ReservePoint.Application.DTOs;

public record ResourceScheduleDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    bool IsAvailableForPeriod,
    IEnumerable<BusySlotDto> BusySlots
);