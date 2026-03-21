using System.Net.Http.Json;
using ReservePoint.Application.Interfaces;

namespace ReservePoint.Infrastructure.Clients;

public class OrgClient : IOrgClient
{
    private readonly HttpClient _httpClient;

    public OrgClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(
            $"api/organizations/{organizationId}/booking-policies", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<OrgPolicyDto>(ct);
    }
}
