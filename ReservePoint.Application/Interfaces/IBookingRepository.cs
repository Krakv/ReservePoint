using ReservePoint.Domain.Entities;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Booking>> GetAllAsync(
        Guid organizationId,
        string? identityId,
        Guid? resourceId,
        DateTime? from,
        DateTime? to,
        BookingStatus? status,
        CancellationToken ct);
    Task<int> CountActiveAsync(string identityId, Guid organizationId, CancellationToken ct);
    Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, CancellationToken ct);
    Task<IEnumerable<Guid>> GetBookedResourceIdsAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct);
    Task AddAsync(Booking booking, CancellationToken ct);
    Task UpdateAsync(Booking booking, CancellationToken ct);
}