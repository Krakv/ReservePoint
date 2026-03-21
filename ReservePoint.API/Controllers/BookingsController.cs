using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservePoint.Application.Interfaces;
using ReservePoint.Application.DTOs;
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

    // GET /v1/bookings?resourceId=&from=&to=&status=
    [HttpGet]
    public async Task<IActionResult> GetBookings(
        [FromQuery] Guid? resourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] BookingStatus? status,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        var identityId = GetIdentityId();

        var bookings = await _bookingService.GetBookingsAsync(
            organizationId, userId, identityId, resourceId, from, to, status, ct);

        return Ok(bookings);
    }

    // GET /v1/bookings/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(Guid id, CancellationToken ct)
    {
        var organizationId = GetOrganizationId();

        var booking = await _bookingService.GetByIdAsync(id, organizationId, ct);

        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    // POST /v1/bookings
    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        var identityId = GetIdentityId();

        var result = await _bookingService.CreateAsync(userId, organizationId, identityId, request, ct);

        if (result.IsFailed)
            return BadRequest(new { error = result.Errors[0].Message });

        return CreatedAtAction(nameof(GetBooking), new { id = result.Value.Id }, result.Value);
    }

    // DELETE /v1/bookings/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        var identityId = GetIdentityId();

        var result = await _bookingService.CancelAsync(id, userId, organizationId, identityId, ct);

        if (result.IsFailed)
            return BadRequest(new { error = result.Errors[0].Message });

        return NoContent();
    }

    // GET /v1/resources/available?from=&to=
    [HttpGet("/v1/resources/available")]
    public async Task<IActionResult> GetAvailableResources(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from >= to)
            return BadRequest(new { error = "'from' должен быть раньше 'to'" });

        var userId = GetUserId();
        var organizationId = GetOrganizationId();
        var identityId = GetIdentityId();

        var resources = await _bookingService.GetAvailableResourcesAsync(
            organizationId, userId, identityId, from, to, ct);

        return Ok(resources);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst("sub")!.Value);
    private Guid GetOrganizationId() => Guid.Parse(User.FindFirst("organizationId")!.Value);
    private string GetIdentityId() => User.FindFirst("sub")!.Value;
}