namespace ReservePoint.Application.DTOs;

public record AvailableResourceDto(
    Guid Id,
    string Name,
    string Type
);
