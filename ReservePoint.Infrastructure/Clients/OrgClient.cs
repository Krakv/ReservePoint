using ReservePoint.Application.DTOs;
using ReservePoint.Application.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace ReservePoint.Infrastructure.Clients;

public class OrgClient : IOrgClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;

    public OrgClient(HttpClient httpClient, IHttpContextAccessor contextAccessor)
    {
        _httpClient = httpClient;
        _contextAccessor = contextAccessor;
    }

    public async Task<OrgPolicyDto?> GetPoliciesAsync(Guid organizationId, CancellationToken ct)
    {
        var token = _contextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v1/organizations/{organizationId}/policy"
        );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<OrgPolicyDto>(cancellationToken: ct);
    }
}
