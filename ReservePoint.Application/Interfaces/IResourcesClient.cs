namespace ReservePoint.Application.Interfaces;

public interface IResourcesClient
{
    Task<ResourceDto?> GetByIdAsync(Guid resourceId, CancellationToken ct);
    Task<IEnumerable<ResourceDto>> GetAvailableAsync(Guid organizationId, CancellationToken ct);
}

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
