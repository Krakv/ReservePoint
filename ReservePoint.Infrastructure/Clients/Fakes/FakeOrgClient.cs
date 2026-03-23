using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients.Fakes;

public class FakeOrgClient : IOrgClient
{
    public Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var policy = new OrgPolicyDto(
            OrganizationId: organizationId,
            MaxBookingDurationMin: 480,
            BookingWindowDays: 30,
            MaxActiveBookingsPerUser: 5
        );

        return Task.FromResult<OrgPolicyDto?>(policy);
    }
}
