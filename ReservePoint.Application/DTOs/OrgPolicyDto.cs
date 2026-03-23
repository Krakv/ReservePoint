using System.Text.Json.Serialization;

namespace ReservePoint.Application.DTOs;

public record OrgPolicyDto(
    [property: JsonPropertyName("organization_id")] Guid OrganizationId,
    [property: JsonPropertyName("max_booking_duration_min")] int MaxBookingDurationMin,
    [property: JsonPropertyName("booking_window_days")] int BookingWindowDays,
    [property: JsonPropertyName("max_active_bookings_per_user")] int MaxActiveBookingsPerUser
);