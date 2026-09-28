using Microsoft.EntityFrameworkCore;
using PaintTint.Core.DTOs;
using PaintTint.Core.Services;
using PaintTint.Infrastructure.Data;

namespace PaintTint.Infrastructure.Services;

public class BaseService : IBaseService
{
    private readonly PaintTintDbContext _context;

    public BaseService(PaintTintDbContext context)
    {
        _context = context;
    }

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
