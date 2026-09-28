using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PaintTint.Core.DTOs;

namespace PaintTint.Wpf.Services;

public interface ITintApiClient
{
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
    Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default);
    Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default);
    Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default);
    Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default);
    Task<List<DispenseJobResponse>> GetRecentJobsAsync(int limit = 10, CancellationToken cancellationToken = default);
}

public class TintApiClient : ITintApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TintApiClient(string baseAddress = "http://localhost:5000")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(8)
        };
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/bases", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/bases", cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<BaseDto>>(JsonOptions, cancellationToken) ?? new();
    }

    public async Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        string uri = string.IsNullOrWhiteSpace(search) ? "api/shades" : $"api/shades?search={Uri.EscapeDataString(search.Trim())}";
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<ShadeSummaryDto>>(JsonOptions, cancellationToken) ?? new();
    }

    public async Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/shades/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ShadeDetailDto>(JsonOptions, cancellationToken);
    }

    public async Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tint/calculate", request, cancellationToken);
        
        // If 400 Bad Request, parse error message from server
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            string userMessage = ExtractErrorMessage(errorContent);
            return new CalculateTintResponse
            {
                IsValid = false,
                ValidationError = userMessage
            };
        }

        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<CalculateTintResponse>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize calculation response.");
    }

    public async Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/dispense-jobs", request, cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            string userMessage = ExtractErrorMessage(errorContent);
            throw new InvalidOperationException(userMessage);
        }

        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<DispenseJobResponse>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize dispense job response.");
    }

    public async Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/dispense-jobs/latest", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<DispenseJobResponse>(JsonOptions, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DispenseJobResponse>> GetRecentJobsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/dispense-jobs/recent?limit={limit}", cancellationToken);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<List<DispenseJobResponse>>(JsonOptions, cancellationToken) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = ExtractErrorMessage(body);
            throw new HttpRequestException($"API request failed with status {response.StatusCode}: {message}");
        }
    }

    private static string ExtractErrorMessage(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent)) return "An unexpected error occurred.";
        try
        {
            using var doc = JsonDocument.Parse(rawContent);
            if (doc.RootElement.TryGetProperty("message", out var msgProp))
            {
                return msgProp.GetString() ?? rawContent;
            }
            if (doc.RootElement.TryGetProperty("title", out var titleProp))
            {
                return titleProp.GetString() ?? rawContent;
            }
        }
        catch
        {
            // fallback
        }
        return rawContent;
    }
}
