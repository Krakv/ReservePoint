namespace ReservePoint.Application.Interfaces;

public interface IOrgClient
{
    Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct);
}

public record OrgPolicyDto(
    Guid OrganizationId,
    int MaxBookingsPerUser,
    TimeOnly AllowedTimeFrom,
    TimeOnly AllowedTimeTo,
    int BookingHorizonDays
);
