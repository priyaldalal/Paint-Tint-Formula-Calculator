using PaintTint.Core.DTOs;

namespace PaintTint.Wpf.Interfaces;

/// <summary>
/// Client contract for communicating with the Paint Tint Web API backend.
/// Handles connection health checks, shade catalog queries, tint calculation,
/// and dispense job operations asynchronously.
/// </summary>
public interface ITintApiClient
{
    /// <summary>
    /// Checks connectivity to the backend Web API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>True if API is responding with success; otherwise false.</returns>
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all shades matching the optional search query.
    /// </summary>
    /// <param name="search">Search text matching shade code or name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of shade summaries with color swatches.</returns>
    Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches full shade details including formulas across different bases.
    /// </summary>
    /// <param name="id">Shade ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detailed shade object or null if not found.</returns>
    Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all available paint bases (Pastel, Medium, Deep) with tint limits and pricing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of base types.</returns>
    Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the formula breakdown, total ml, tint %, and price for a shade and can size.
    /// </summary>
    /// <param name="request">ShadeId, BaseId, and CanSizeLitres.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Calculation response with itemized breakdown and validation status.</returns>
    Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a new dispense job in the backend database.
    /// </summary>
    /// <param name="request">Dispense parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recorded dispense job with generated ID.</returns>
    Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the most recent dispense job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest dispense job record, or null if none exist.</returns>
    Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves recent dispense jobs for display in the history view.
    /// </summary>
    /// <param name="limit">Number of records to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of recent dispense jobs.</returns>
    Task<List<DispenseJobResponse>> GetRecentJobsAsync(int limit = 10, CancellationToken cancellationToken = default);
}
