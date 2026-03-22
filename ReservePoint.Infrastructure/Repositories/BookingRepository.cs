using Microsoft.EntityFrameworkCore;
using ReservePoint.Application.Interfaces;
using ReservePoint.Domain.Entities;
using ReservePoint.Domain.Enums;
using ReservePoint.Infrastructure.Persistence;

namespace ReservePoint.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(
        Guid organizationId,
        string? identityId,
        Guid? resourceId,
        DateTime? from,
        DateTime? to,
        BookingStatus? status,
        CancellationToken ct)
    {
        var query = _context.Bookings
            .Where(b => b.OrganizationId == organizationId);

        if (identityId is not null)
            query = query.Where(b => b.IdentityId == identityId);

        if (resourceId.HasValue)
            query = query.Where(b => b.ResourceId == resourceId.Value);

        if (from.HasValue)
            query = query.Where(b => b.EndTime >= from.Value);

        if (to.HasValue)
            query = query.Where(b => b.StartTime <= to.Value);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        return await query.ToListAsync(ct);
    }

    public async Task<int> CountActiveAsync(string identityId, Guid organizationId, CancellationToken ct)
    {
        return await _context.Bookings
            .CountAsync(b =>
                b.IdentityId == identityId &&
                b.OrganizationId == organizationId &&
                b.Status == BookingStatus.Active, ct);
    }

    public async Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, CancellationToken ct)
    {
        Console.WriteLine($"HasConflict check: {start.Kind} {start} - {end.Kind} {end}");
        return await _context.Bookings
            .AnyAsync(b =>
                b.ResourceId == resourceId &&
                b.Status == BookingStatus.Active &&
                b.StartTime < end &&
                b.EndTime > start, ct);
    }

    public async Task<IEnumerable<Guid>> GetBookedResourceIdsAsync(
        Guid organizationId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _context.Bookings
            .Where(b =>
                b.OrganizationId == organizationId &&
                b.Status == BookingStatus.Active &&
                b.StartTime < to &&
                b.EndTime > from)
            .Select(b => b.ResourceId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct)
    {
        await _context.Bookings.AddAsync(booking, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(ct);
    }
}