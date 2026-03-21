using System.Net.Http.Json;
using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients;

public class UserClient : IUserClient
{
    private readonly HttpClient _httpClient;

    public UserClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserMembershipDto?> GetMembershipAsync(
        Guid organizationId,
        string identityId,
        CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(
            $"api/internal/organizations/{organizationId}/users/{identityId}/membership", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserMembershipDto>(ct);
    }

    public async Task<bool> CheckPermissionAsync(
        Guid organizationId,
        string identityId,
        string permissionCode,
        CancellationToken ct)
    {
        var body = new { organizationId, identityId, permissionCode };

        var response = await _httpClient.PostAsJsonAsync(
            "api/internal/authorization/check", body, ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content
            .ReadFromJsonAsync<CheckPermissionResponse>(ct);

        return result?.Allowed ?? false;
    }
}

file sealed record CheckPermissionResponse(bool Allowed);
