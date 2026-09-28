using Microsoft.EntityFrameworkCore;
using PaintTint.Core.DTOs;
using PaintTint.Core.Interfaces;
using PaintTint.Infrastructure.Data;

namespace PaintTint.Infrastructure.Services;

/// <summary>
/// Service implementation for querying paint bases from the database.
/// </summary>
public class BaseService : IBaseService
{
    private readonly PaintTintDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BaseService(PaintTintDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bases
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new BaseDto
            {
                Id = b.Id,
                Name = b.Name,
                MaxTintPercent = b.MaxTintPercent,
                PricePerLitre = b.PricePerLitre
            })
            .ToListAsync(cancellationToken);
    }
}
