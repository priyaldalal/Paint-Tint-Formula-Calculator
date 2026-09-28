using Microsoft.EntityFrameworkCore;
using PaintTint.Core.DTOs;
using PaintTint.Core.Entities;
using PaintTint.Core.Exceptions;
using PaintTint.Core.Interfaces;
using PaintTint.Infrastructure.Data;

namespace PaintTint.Infrastructure.Services;

/// <summary>
/// Service implementation for tint formula calculation and dispense job persistence.
/// </summary>
public class DispenseService : IDispenseService
{
    private readonly PaintTintDbContext _context;
    private readonly ITintCalculator _tintCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispenseService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="tintCalculator">The mathematical tint calculation engine.</param>
    public DispenseService(PaintTintDbContext context, ITintCalculator tintCalculator)
    {
        _context = context;
        _tintCalculator = tintCalculator;
    }

    /// <inheritdoc/>
    public async Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var shade = await _context.Shades
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.ShadeId, cancellationToken);

        if (shade == null)
        {
            throw new NotFoundException($"Shade with ID {request.ShadeId} was not found.");
        }

        var basePaint = await _context.Bases
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BaseId, cancellationToken);

        if (basePaint == null)
        {
            throw new NotFoundException($"Base with ID {request.BaseId} was not found.");
        }

        var formulaItems = await _context.FormulaItems
            .AsNoTracking()
            .Include(fi => fi.Colorant)
            .Where(fi => fi.ShadeId == request.ShadeId && fi.BaseId == request.BaseId)
            .ToListAsync(cancellationToken);

        if (formulaItems.Count == 0)
        {
            throw new NotFoundException($"No formula available for shade '{shade.Name}' ({shade.Code}) with base '{basePaint.Name}'.");
        }

        return _tintCalculator.Calculate(shade, basePaint, request.CanSizeLitres, formulaItems);
    }

    /// <inheritdoc/>
    public async Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var calculateResponse = await CalculateAsync(new CalculateTintRequest
        {
            ShadeId = request.ShadeId,
            BaseId = request.BaseId,
            CanSizeLitres = request.CanSizeLitres
        }, cancellationToken);

        if (!calculateResponse.IsValid)
        {
            throw new TintValidationException(calculateResponse.ValidationError ?? "Tint formula exceeds base limits.");
        }

        var job = new DispenseJob
        {
            ShadeId = calculateResponse.ShadeId,
            BaseId = calculateResponse.BaseId,
            CanSizeLitres = calculateResponse.CanSizeLitres,
            TotalColorantMl = calculateResponse.TotalColorantMl,
            TintPercent = calculateResponse.TintPercent,
            TotalPrice = calculateResponse.TotalPrice,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in calculateResponse.Items)
        {
            job.Items.Add(new DispenseJobItem
            {
                ColorantId = item.ColorantId,
                DispensedMl = item.ScaledMl,
                Cost = item.Cost
            });
        }

        _context.DispenseJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapToJobResponseAsync(job.Id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DispenseJobResponse>> GetRecentJobsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var jobs = await _context.DispenseJobs
            .AsNoTracking()
            .Include(j => j.Shade)
            .Include(j => j.Base)
            .Include(j => j.Items)
                .ThenInclude(i => i.Colorant)
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

        return jobs.Select(MapJob).ToList();
    }

    /// <inheritdoc/>
    public async Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default)
    {
        var job = await _context.DispenseJobs
            .AsNoTracking()
            .Include(j => j.Shade)
            .Include(j => j.Base)
            .Include(j => j.Items)
                .ThenInclude(i => i.Colorant)
            .OrderByDescending(j => j.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return job == null ? null : MapJob(job);
    }

    private async Task<DispenseJobResponse> MapToJobResponseAsync(int jobId, CancellationToken cancellationToken)
    {
        var job = await _context.DispenseJobs
            .AsNoTracking()
            .Include(j => j.Shade)
            .Include(j => j.Base)
            .Include(j => j.Items)
                .ThenInclude(i => i.Colorant)
            .FirstAsync(j => j.Id == jobId, cancellationToken);

        return MapJob(job);
    }

    private static DispenseJobResponse MapJob(DispenseJob job)
    {
        return new DispenseJobResponse
        {
            Id = job.Id,
            ShadeId = job.ShadeId,
            ShadeCode = job.Shade?.Code ?? string.Empty,
            ShadeName = job.Shade?.Name ?? string.Empty,
            BaseId = job.BaseId,
            BaseName = job.Base?.Name ?? string.Empty,
            CanSizeLitres = job.CanSizeLitres,
            TotalColorantMl = job.TotalColorantMl,
            TintPercent = job.TintPercent,
            TotalPrice = job.TotalPrice,
            CreatedAtUtc = job.CreatedAtUtc,
            Items = job.Items.Select(i => new DispenseJobItemResponse
            {
                Id = i.Id,
                ColorantId = i.ColorantId,
                ColorantCode = i.Colorant?.Code ?? string.Empty,
                ColorantName = i.Colorant?.Name ?? string.Empty,
                DispensedMl = i.DispensedMl,
                Cost = i.Cost
            }).ToList()
        };
    }
}
