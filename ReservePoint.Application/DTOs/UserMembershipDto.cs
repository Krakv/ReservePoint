namespace ReservePoint.Application.DTOs;

public record UserMembershipDto(
    Guid MembershipId,
    string Status,
    IEnumerable<string> Roles
);
