using PaintTint.Core.DTOs;
using PaintTint.Core.Entities;
using PaintTint.Core.Exceptions;
using PaintTint.Core.Interfaces;

namespace PaintTint.Core.Services;

/// <summary>
/// Implements domain calculation logic for paint tint dispensing:
/// linear scaling across container sizes, rounding to nearest 0.05 ml dispenser unit,
/// validating base maximum tint percentage limits, and calculating total cost.
/// </summary>
public class TintCalculator : ITintCalculator
{
    /// <summary>
    /// Valid container sizes allowed by business rules (in litres).
    /// </summary>
    private static readonly HashSet<decimal> ValidCanSizes = new() { 1m, 4m, 10m, 20m };

    /// <summary>
    /// Rounds a scaled colorant volume to the nearest 0.05 ml (the dispensing machine's physical resolution).
    /// Formula: Round(amount * 20, AwayFromZero) / 20.
    /// </summary>
    /// <param name="ml">The raw colorant amount in milliliters.</param>
    /// <returns>The rounded amount in milliliters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ml"/> is negative.</exception>
    public decimal RoundToDispenserUnit(decimal ml)
    {
        if (ml < 0m)
            throw new ArgumentOutOfRangeException(nameof(ml), "Colorant amount cannot be negative.");

        // Multiply by 20 to convert 0.05 units into whole integers, round away from zero, then divide by 20
        return Math.Round(ml * 20m, MidpointRounding.AwayFromZero) / 20m;
    }

    /// <summary>
    /// Calculates the scaled colorant breakdown, tint percentage, and price for a shade formulation.
    /// </summary>
    /// <param name="shade">The shade entity being tinted.</param>
    /// <param name="basePaint">The base paint (e.g. Pastel, Medium, Deep).</param>
    /// <param name="canSizeLitres">Can size in litres (must be 1, 4, 10, or 20 L).</param>
    /// <param name="formulaItems">Formula items specifying colorant ml per litre.</param>
    /// <returns>A detailed calculation response with itemized costs and validation status.</returns>
    /// <exception cref="ArgumentNullException">Thrown if shade, basePaint, or formulaItems is null.</exception>
    /// <exception cref="TintValidationException">Thrown if can size is not 1, 4, 10, or 20 L, or if formula is empty.</exception>
    public CalculateTintResponse Calculate(
        Shade shade,
        Base basePaint,
        decimal canSizeLitres,
        IEnumerable<FormulaItem> formulaItems)
    {
        ArgumentNullException.ThrowIfNull(shade);
        ArgumentNullException.ThrowIfNull(basePaint);
        ArgumentNullException.ThrowIfNull(formulaItems);

        // Rule 2: Can sizes are 1L, 4L, 10L, and 20L
        if (!ValidCanSizes.Contains(canSizeLitres))
        {
            throw new TintValidationException($"Invalid can size: {canSizeLitres}L. Allowed sizes are 1L, 4L, 10L, and 20L.");
        }

        var formulaList = formulaItems.ToList();
        if (formulaList.Count == 0)
        {
            throw new TintValidationException($"No formula items found for shade '{shade.Name}' ({shade.Code}) with base '{basePaint.Name}'.");
        }

        var calculatedItems = new List<CalculatedColorantItemDto>();
        decimal totalColorantMl = 0m;
        decimal colorantsCost = 0m;

        foreach (var item in formulaList)
        {
            if (item.Colorant == null)
            {
                throw new InvalidOperationException($"Colorant reference missing for formula item ID {item.Id}.");
            }

            // Rules 2 & 3: Amounts scale linearly with can size, rounded to nearest 0.05 ml
            decimal rawScaledMl = item.MlPerLitre * canSizeLitres;
            decimal scaledMl = RoundToDispenserUnit(rawScaledMl);

            // Rule 5: Colorant cost = colorant ml * colorant cost per ml, rounded to 2 decimals
            decimal itemCost = Math.Round(scaledMl * item.Colorant.CostPerMl, 2, MidpointRounding.AwayFromZero);

            totalColorantMl += scaledMl;
            colorantsCost += itemCost;

            calculatedItems.Add(new CalculatedColorantItemDto
            {
                ColorantId = item.ColorantId,
                Code = item.Colorant.Code,
                Name = item.Colorant.Name,
                MlPerLitre = item.MlPerLitre,
                ScaledMl = scaledMl,
                CostPerMl = item.Colorant.CostPerMl,
                Cost = itemCost
            });
        }

        // Rule 4: Total colorant must not exceed max tint % of the can volume
        // Can volume in ml = canSizeLitres * 1000 ml
        decimal canVolumeMl = canSizeLitres * 1000m;
        decimal tintPercent = Math.Round((totalColorantMl / canVolumeMl) * 100m, 2, MidpointRounding.AwayFromZero);
        decimal maxAllowedColorantMl = Math.Round((basePaint.MaxTintPercent / 100m) * canVolumeMl, 2, MidpointRounding.AwayFromZero);

        // Rule 5: Price = base price per litre * litres + sum of (colorant ml * colorant cost per ml)
        decimal baseCost = Math.Round(basePaint.PricePerLitre * canSizeLitres, 2, MidpointRounding.AwayFromZero);
        decimal totalPrice = baseCost + colorantsCost;

        bool exceedsLimit = totalColorantMl > maxAllowedColorantMl;
        string? validationError = null;

        if (exceedsLimit)
        {
            validationError = $"Tint of {totalColorantMl:F2} ml ({tintPercent:F2}%) exceeds {basePaint.MaxTintPercent:G29}% limit for {basePaint.Name} base (maximum allowed: {maxAllowedColorantMl:F2} ml).";
        }

        return new CalculateTintResponse
        {
            ShadeId = shade.Id,
            ShadeCode = shade.Code,
            ShadeName = shade.Name,
            HexColor = shade.HexColor,
            BaseId = basePaint.Id,
            BaseName = basePaint.Name,
            MaxTintPercent = basePaint.MaxTintPercent,
            CanSizeLitres = canSizeLitres,
            BasePricePerLitre = basePaint.PricePerLitre,
            BaseCost = baseCost,
            ColorantsCost = colorantsCost,
            TotalColorantMl = totalColorantMl,
            TintPercent = tintPercent,
            MaxAllowedColorantMl = maxAllowedColorantMl,
            TotalPrice = totalPrice,
            IsValid = !exceedsLimit,
            ValidationError = validationError,
            Items = calculatedItems
        };
    }
}
