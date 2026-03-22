using ReservePoint.Application.DTOs;
using ReservePoint.Domain.Entities;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Interfaces;

public interface IBookingRepository
{
    Task<BookingGroup?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<BookingGroup>> GetAllAsync(
        Guid organizationId,
        string? identityId,
        DateTime? from,
        DateTime? to,
        BookingGroupStatus? status,
        CancellationToken ct);
    Task<int> CountActiveAsync(string identityId, Guid organizationId, CancellationToken ct);
    Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, CancellationToken ct);
    Task<IEnumerable<Guid>> GetBookedResourceIdsAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct);
    Task AddAsync(BookingGroup bookingGroup, CancellationToken ct);
    Task UpdateAsync(BookingGroup bookingGroup, CancellationToken ct);

    Task<IEnumerable<BusySlotDto>> GetBusySlotsAsync(
        Guid resourceId,
        DateTime from,
        DateTime to,
        CancellationToken ct);
}
