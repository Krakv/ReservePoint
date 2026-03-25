using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients.Fakes;

public class FakeOrgClient : IOrgClient
{
    public Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var policy = new OrgPolicyDto(
            ID: 1,
            OrganizationID: organizationId,
            MaxBookingDurationMin: 480,
            BookingWindowDays: 30,
            MaxActiveBookingsPerUser: 5,
            CreatedAt: now,
            UpdatedAt: now
        );

        return Task.FromResult<OrgPolicyDto?>(policy);
    }
}