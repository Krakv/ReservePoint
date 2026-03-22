using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients.Fakes;

public class FakeOrgClient : IOrgClient
{
    public Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var policy = new OrgPolicyDto(
            organizationId,
            MaxBookingsPerUser: 5,
            AllowedTimeFrom: new TimeOnly(8, 0),
            AllowedTimeTo: new TimeOnly(20, 0),
            BookingHorizonDays: 14
        );

        return Task.FromResult<OrgPolicyDto?>(policy);
    }
}
