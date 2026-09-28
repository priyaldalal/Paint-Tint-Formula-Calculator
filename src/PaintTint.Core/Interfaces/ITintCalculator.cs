using PaintTint.Core.DTOs;
using PaintTint.Core.Entities;

namespace PaintTint.Core.Interfaces;

/// <summary>
/// Defines mathematical calculation and scaling operations for paint tint formulas,
/// including dispenser unit rounding, tint percentage limit validation, and pricing.
/// </summary>
public interface ITintCalculator
{
    /// <summary>
    /// Rounds a colorant amount to the nearest 0.05 ml (the dispenser machine's smallest unit).
    /// </summary>
    /// <param name="ml">The raw calculated colorant amount in milliliters.</param>
    /// <returns>The rounded amount in milliliters to the nearest 0.05 ml.</returns>
    decimal RoundToDispenserUnit(decimal ml);

    /// <summary>
    /// Calculates the scaled colorant quantities, total tint percentage, validation against base limits,
    /// and total customer price for a given shade, base, and can size.
    /// </summary>
    /// <param name="shade">The shade entity being tinted.</param>
    /// <param name="basePaint">The base paint entity (e.g. Pastel, Medium, Deep).</param>
    /// <param name="canSizeLitres">The container volume in litres (1, 4, 10, or 20 L).</param>
    /// <param name="formulaItems">The collection of formula items specifying colorant ml per litre.</param>
    /// <returns>A detailed <see cref="CalculateTintResponse"/> with scaled items, costs, and validation status.</returns>
    CalculateTintResponse Calculate(
        Shade shade,
        Base basePaint,
        decimal canSizeLitres,
        IEnumerable<FormulaItem> formulaItems);
}
