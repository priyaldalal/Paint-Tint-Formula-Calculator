using Microsoft.EntityFrameworkCore;
using PaintTint.Core.DTOs;
using PaintTint.Core.Services;
using PaintTint.Infrastructure.Data;

namespace PaintTint.Infrastructure.Services;

public class ShadeService : IShadeService
{
    private readonly PaintTintDbContext _context;

    public ShadeService(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Shades.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string trimmed = search.Trim();
            query = query.Where(s => EF.Functions.Like(s.Name, $"%{trimmed}%") || EF.Functions.Like(s.Code, $"%{trimmed}%"));
        }

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new ShadeSummaryDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                HexColor = s.HexColor
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var shade = await _context.Shades
            .AsNoTracking()
            .Include(s => s.FormulaItems)
                .ThenInclude(fi => fi.Base)
            .Include(s => s.FormulaItems)
                .ThenInclude(fi => fi.Colorant)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shade == null) return null;

        var formulasByBase = shade.FormulaItems
            .GroupBy(fi => new { fi.BaseId, BaseName = fi.Base?.Name ?? string.Empty })
            .Select(g => new BaseFormulaDto
            {
                BaseId = g.Key.BaseId,
                BaseName = g.Key.BaseName,
                Items = g.Select(i => new FormulaItemDto
                {
                    ColorantId = i.ColorantId,
                    ColorantCode = i.Colorant?.Code ?? string.Empty,
                    ColorantName = i.Colorant?.Name ?? string.Empty,
                    MlPerLitre = i.MlPerLitre,
                    CostPerMl = i.Colorant?.CostPerMl ?? 0m
                }).ToList()
            })
            .ToList();

        return new ShadeDetailDto
        {
            Id = shade.Id,
            Code = shade.Code,
            Name = shade.Name,
            HexColor = shade.HexColor,
            Formulas = formulasByBase
        };
    }
}
