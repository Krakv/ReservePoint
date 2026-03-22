using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients.Fakes;

public class FakeUserClient : IUserClient
{
    // Все права разрешены для тестирования
    private static readonly HashSet<string> AllPermissions =
    [
        "BOOKINGS_CREATE",
        "BOOKINGS_READ",
        "BOOKINGS_SEARCH",
        "BOOKINGS_CANCEL_OWN",
        "BOOKINGS_MANAGE_ANY"
    ];

    public Task<UserMembershipDto?> GetMembershipAsync(
        Guid organizationId, string identityId, CancellationToken ct)
    {
        var membership = new UserMembershipDto(
            MembershipId: Guid.NewGuid(),
            Status: "Active",
            Roles: ["EMPLOYEE"]
        );

        return Task.FromResult<UserMembershipDto?>(membership);
    }

    public Task<bool> CheckPermissionAsync(
        Guid organizationId, string identityId, string permissionCode, CancellationToken ct)
    {
        return Task.FromResult(AllPermissions.Contains(permissionCode));
    }
}
