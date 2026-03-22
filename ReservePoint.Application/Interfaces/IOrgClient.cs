using ReservePoint.Application.DTOs;

namespace ReservePoint.Application.Interfaces;

public interface IOrgClient
{
    Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct);
}
