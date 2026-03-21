namespace ReservePoint.Application.DTOs;

public record ResourceDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    ResourceBookingRulesDto BookingRules
);

public record ResourceBookingRulesDto(
    int MaxDurationHours,
    IEnumerable<string> AllowedRoles
);
