using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;
using ReservePoint.Domain.Enums;

namespace ReservePoint.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // GET /v1/bookings?organizationId=&from=&to=&status=
    [HttpGet]
    public async Task<IActionResult> GetBookings(
        [FromQuery] Guid organizationId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] BookingGroupStatus? status,
        CancellationToken ct)
    {
        var identityId = GetIdentityId();

        var bookings = await _bookingService.GetBookingsAsync(
            organizationId, identityId,
            from.HasValue ? ToUtc(from.Value) : null,
            to.HasValue ? ToUtc(to.Value) : null,
            status, ct);

        return Ok(bookings);
    }

    // GET /v1/bookings/{id}?organizationId=
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(
        Guid id,
        [FromQuery] Guid organizationId,
        CancellationToken ct)
    {
        var booking = await _bookingService.GetByIdAsync(id, organizationId, ct);

        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    // POST /v1/bookings
    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingGroupRequest request,
        CancellationToken ct)
    {
        var identityId = GetIdentityId();

        var utcRequest = request with
        {
            StartTime = ToUtc(request.StartTime),
            EndTime = ToUtc(request.EndTime)
        };

        var result = await _bookingService.CreateAsync(identityId, utcRequest, ct);

        if (result.IsFailed)
            return BadRequest(new { error = result.Errors.First().Message });

        return CreatedAtAction(nameof(GetBooking),
            new { id = result.Value.Id, organizationId = result.Value.OrganizationId },
            result.Value);
    }

    // DELETE /v1/bookings/{id}?organizationId=
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelBooking(
        Guid id,
        [FromQuery] Guid organizationId,
        CancellationToken ct)
    {
        var identityId = GetIdentityId();

        var result = await _bookingService.CancelAsync(id, identityId, organizationId, ct);

        if (result.IsFailed)
            return BadRequest(new { error = result.Errors.First().Message });

        return NoContent();
    }

    // GET /v1/resources/available?organizationId=&from=&to=
    [HttpGet("/v1/resources/available")]
    public async Task<IActionResult> GetAvailableResources(
        [FromQuery] Guid organizationId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from >= to)
            return BadRequest(new { error = "'from' должен быть раньше 'to'" });

        var identityId = GetIdentityId();

        var resources = await _bookingService.GetAvailableResourcesAsync(
            organizationId, identityId, ToUtc(from), ToUtc(to), ct);

        return Ok(resources);
    }

    // GET /v1/resources/{resourceId}/busy-slots?from=&to=
    [HttpGet("/v1/resources/{resourceId:guid}/busy-slots")]
    public async Task<IActionResult> GetBusySlots(
    Guid resourceId,
    [FromQuery] Guid organizationId,
    [FromQuery] DateTime from,
    [FromQuery] DateTime to,
    CancellationToken ct)
    {
        var slots = await _bookingService.GetBusySlotsAsync(
            resourceId, organizationId, ToUtc(from), ToUtc(to), ct);
        return Ok(slots);
    }

    [HttpGet("/v1/resources/schedule")]
    public async Task<IActionResult> GetResourcesSchedule(
    [FromQuery] Guid organizationId,
    [FromQuery] DateTime from,
    [FromQuery] DateTime to,
    CancellationToken ct)
    {
        if (from >= to)
            return BadRequest(new { error = "'from' должен быть раньше 'to'" });

        var identityId = GetIdentityId();
        var schedule = await _bookingService.GetResourcesScheduleAsync(
            organizationId, identityId, ToUtc(from), ToUtc(to), ct);

        return Ok(schedule);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    private string GetIdentityId() =>
        User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value;

    private static DateTime ToUtc(DateTime dt) =>
        DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
