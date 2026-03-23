using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ReservePoint.Infrastructure.Clients;

public class OrgClient : IOrgClient
{
    private readonly HttpClient _httpClient;
    private readonly string _jwtToken;

    public OrgClient(HttpClient httpClient, string jwtToken)
    {
        _httpClient = httpClient;
        _jwtToken = jwtToken;
    }

    public async Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v1/organizations/{organizationId}/policy"
        );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<OrgPolicyDto>(cancellationToken: ct);
    }
}
