using FluentResults;
using ReservePoint.Application.DTOs;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingGroupDto>> GetBookingsAsync(
        Guid organizationId,
        string identityId,
        DateTime? from,
        DateTime? to,
        BookingGroupStatus? status,
        CancellationToken ct);

    Task<BookingGroupDto?> GetByIdAsync(
        Guid id,
        Guid organizationId,
        CancellationToken ct);

    Task<Result<BookingGroupDto>> CreateAsync(
        string identityId,
        CreateBookingGroupRequest request,
        CancellationToken ct);

    Task<Result> CancelAsync(
        Guid bookingGroupId,
        string identityId,
        Guid organizationId,
        CancellationToken ct);

    Task<IEnumerable<AvailableResourceDto>> GetAvailableResourcesAsync(
        Guid organizationId,
        string identityId,
        DateTime from,
        DateTime to,
        CancellationToken ct);
}
