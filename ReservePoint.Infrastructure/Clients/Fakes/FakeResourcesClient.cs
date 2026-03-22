using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients.Fakes;

public class FakeResourcesClient : IResourcesClient
{
    private static readonly List<ResourceDto> Resources =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Переговорная А",
            "meeting_room",
            "available",
            new ResourceBookingRulesDto(4, ["EMPLOYEE", "RESOURCE_ADMIN", "ORG_OWNER"])
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Переговорная Б",
            "meeting_room",
            "available",
            new ResourceBookingRulesDto(8, ["EMPLOYEE", "RESOURCE_ADMIN", "ORG_OWNER"])
        ),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Рабочее место 1",
            "workspace",
            "unavailable",
            new ResourceBookingRulesDto(12, ["EMPLOYEE", "RESOURCE_ADMIN", "ORG_OWNER"])
        )
    ];

    public Task<ResourceDto?> GetByIdAsync(Guid resourceId, CancellationToken ct)
    {
        var resource = Resources.FirstOrDefault(r => r.Id == resourceId);
        return Task.FromResult(resource);
    }

    public Task<IEnumerable<ResourceDto>> GetAvailableAsync(Guid organizationId, CancellationToken ct)
    {
        var available = Resources.Where(r => r.Status == "available");
        return Task.FromResult(available);
    }
}
