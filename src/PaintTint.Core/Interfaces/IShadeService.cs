using PaintTint.Core.DTOs;

namespace PaintTint.Core.Interfaces;

/// <summary>
/// Provides operations for querying paint shades and their associated base formulas.
/// </summary>
public interface IShadeService
{
    /// <summary>
    /// Retrieves a list of available shades, optionally filtered by name or code.
    /// </summary>
    /// <param name="search">Optional query string matching shade code or name.</param>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>A list of matching shades with hex color codes.</returns>
    Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information for a specific shade, including formulas for each supported paint base.
    /// </summary>
    /// <param name="id">The unique identifier of the shade.</param>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>Detailed shade object or null if not found.</returns>
    Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default);
}
