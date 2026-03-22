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

    public async Task<BookingGroup?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.BookingGroups
            .Include(g => g.Bookings)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<IEnumerable<BookingGroup>> GetAllAsync(
        Guid organizationId,
        string? identityId,
        DateTime? from,
        DateTime? to,
        BookingGroupStatus? status,
        CancellationToken ct)
    {
        var query = _context.BookingGroups
            .Include(g => g.Bookings)
            .Where(g => g.OrganizationId == organizationId);

        if (identityId is not null)
            query = query.Where(g => g.IdentityId == identityId);

        if (from.HasValue)
            query = query.Where(g => g.EndTime >= from.Value);

        if (to.HasValue)
            query = query.Where(g => g.StartTime <= to.Value);

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync(ct);
    }

    public async Task<int> CountActiveAsync(string identityId, Guid organizationId, CancellationToken ct)
    {
        return await _context.BookingGroups
            .CountAsync(g =>
                g.IdentityId == identityId &&
                g.OrganizationId == organizationId &&
                g.Status == BookingGroupStatus.Active, ct);
    }

    public async Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, CancellationToken ct)
    {
        return await _context.BookingGroups
            .Where(g =>
                g.Status == BookingGroupStatus.Active &&
                g.StartTime < end &&
                g.EndTime > start)
            .AnyAsync(g => g.Bookings.Any(b =>
                b.ResourceId == resourceId &&
                b.Status == BookingStatus.Active), ct);
    }

    public async Task<IEnumerable<Guid>> GetBookedResourceIdsAsync(
        Guid organizationId, DateTime from, DateTime to, CancellationToken ct)
    {
        return await _context.BookingGroups
            .Where(g =>
                g.OrganizationId == organizationId &&
                g.Status == BookingGroupStatus.Active &&
                g.StartTime < to &&
                g.EndTime > from)
            .SelectMany(g => g.Bookings)
            .Where(b => b.Status == BookingStatus.Active)
            .Select(b => b.ResourceId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task AddAsync(BookingGroup bookingGroup, CancellationToken ct)
    {
        await _context.BookingGroups.AddAsync(bookingGroup, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BookingGroup bookingGroup, CancellationToken ct)
    {
        _context.BookingGroups.Update(bookingGroup);
        await _context.SaveChangesAsync(ct);
    }
}
