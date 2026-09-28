using PaintTint.Core.DTOs;

namespace PaintTint.Core.Interfaces;

/// <summary>
/// Provides operations for calculating paint tint formulations and recording physical dispense jobs in the database.
/// </summary>
public interface IDispenseService
{
    /// <summary>
    /// Calculates the scaled colorant breakdown, total tint percentage, and total price for a given shade and container size.
    /// </summary>
    /// <param name="request">The calculation request containing ShadeId, BaseId, and CanSizeLitres.</param>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>A calculation response containing itemized colorants, total volume, tint %, price, and validation status.</returns>
    Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and records a new dispense job in the database with historical itemized costs and dispensed quantities.
    /// </summary>
    /// <param name="request">The dispense job creation request.</param>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>The created dispense job record with generated ID and timestamps.</returns>
    Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves recent dispense jobs for audit and history tracking.
    /// </summary>
    /// <param name="count">The maximum number of recent records to return (defaults to 10).</param>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>A list of recent dispense jobs ordered by creation timestamp descending.</returns>
    Task<List<DispenseJobResponse>> GetRecentJobsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the most recently completed dispense job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>The latest dispense job, or null if no jobs have been recorded.</returns>
    Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default);
}
