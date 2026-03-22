using System.Net.Http.Json;
using ReservePoint.Application.Interfaces;
using ReservePoint.Application.DTOs;

namespace ReservePoint.Infrastructure.Clients;

public class ResourcesClient : IResourcesClient
{
    private readonly HttpClient _httpClient;

    public ResourcesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResourceDto?> GetByIdAsync(Guid resourceId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"api/resources/{resourceId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ResourceDto>(ct);
    }

    public async Task<IEnumerable<ResourceDto>> GetAvailableAsync(Guid organizationId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(
            $"api/resources?organizationId={organizationId}&status=available", ct);

        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<ResourceDto>();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ResourceDto>>(ct)
               ?? Enumerable.Empty<ResourceDto>();
    }

    public async Task<IEnumerable<ResourceDto>> GetAllAsync(Guid organizationId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(
            $"api/resources?organizationId={organizationId}", ct);

        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<ResourceDto>();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ResourceDto>>(ct)
               ?? Enumerable.Empty<ResourceDto>();
    }
}
