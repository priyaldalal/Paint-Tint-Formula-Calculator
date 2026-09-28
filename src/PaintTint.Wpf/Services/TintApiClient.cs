using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PaintTint.Core.DTOs;
using PaintTint.Wpf.Interfaces;

namespace PaintTint.Wpf.Services;

/// <summary>
/// HTTP client implementation for communicating with the Paint Tint Web API backend.
/// Features resilient error handling, automatic JSON deserialization, and timeout management.
/// </summary>
public class TintApiClient : ITintApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="TintApiClient"/> class.
    /// </summary>
    /// <param name="baseAddress">The base URL of the API server (defaults to http://localhost:5000).</param>
    public TintApiClient(string baseAddress = "http://localhost:5000")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(8)
        };
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/bases", cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<BaseDto>>(JsonOptions, cancellationToken) ?? new();
    }

    /// <inheritdoc/>
    public async Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        string uri = string.IsNullOrWhiteSpace(search) 
            ? "api/shades" 
            : $"api/shades?search={Uri.EscapeDataString(search.Trim())}";
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<ShadeSummaryDto>>(JsonOptions, cancellationToken) ?? new();
    }

    /// <inheritdoc/>
    public async Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/shades/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ShadeDetailDto>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tint/calculate", request, cancellationToken);
        
        // If 400 Bad Request, parse friendly validation error message from server response
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            // fallback to raw text if not valid JSON
        }
        return rawContent;
    }
}
