using FluentResults;
using ReservePoint.Application.DTOs;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetBookingsAsync(
        Guid organizationId,
        string identityId,
        Guid? resourceId,
        DateTime? from,
        DateTime? to,
        BookingStatus? status,
        CancellationToken ct);

    Task<BookingDto?> GetByIdAsync(
        Guid id,
        Guid organizationId,
        CancellationToken ct);

    Task<Result<BookingDto>> CreateAsync(
        string identityId,
        CreateBookingRequest request,
        CancellationToken ct);

    Task<Result> CancelAsync(
        Guid bookingId,
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