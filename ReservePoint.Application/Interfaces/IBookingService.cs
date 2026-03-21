using FluentResults;
using ReservePoint.Application.DTOs;
using ReservePoint.Domain.Enums;

namespace ReservePoint.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetBookingsAsync(
        Guid organizationId,
        Guid userId,
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
        Guid userId,
        Guid organizationId,
        string identityId,
        CreateBookingRequest request,
        CancellationToken ct);

    Task<Result> CancelAsync(
        Guid bookingId,
        Guid userId,
        Guid organizationId,
        string identityId,
        CancellationToken ct);

    Task<IEnumerable<AvailableResourceDto>> GetAvailableResourcesAsync(
        Guid organizationId,
        Guid userId,
        string identityId,
        DateTime from,
        DateTime to,
        CancellationToken ct);
}