using PaintTint.Core.DTOs;

namespace PaintTint.Core.Interfaces;

/// <summary>
/// Provides operations for querying paint base products (Pastel, Medium, Deep) and their pricing/tint limits.
/// </summary>
public interface IBaseService
{
    /// <summary>
    /// Retrieves all registered paint bases with maximum tint percentages and price per litre.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async execution.</param>
    /// <returns>A list of available paint bases.</returns>
    Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default);
}
