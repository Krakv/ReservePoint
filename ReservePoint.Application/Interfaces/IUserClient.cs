using ReservePoint.Application.DTOs;

namespace ReservePoint.Application.Interfaces;

public interface IUserClient
{
    Task<UserMembershipDto?> GetMembershipAsync(
        Guid organizationId,
        string identityId,
        CancellationToken ct);

    Task<bool> CheckPermissionAsync(
        Guid organizationId,
        string identityId,
        string permissionCode,
        CancellationToken ct);
}
