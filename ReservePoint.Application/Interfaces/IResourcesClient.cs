using ReservePoint.Application.DTOs;

namespace ReservePoint.Application.Interfaces;

public interface IResourcesClient
{
    Task<ResourceDto?> GetByIdAsync(Guid resourceId, CancellationToken ct);
    Task<IEnumerable<ResourceDto>> GetAvailableAsync(Guid organizationId, CancellationToken ct);
    Task<IEnumerable<ResourceDto>> GetAllAsync(Guid organizationId, CancellationToken ct);
}
