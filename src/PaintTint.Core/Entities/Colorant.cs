namespace PaintTint.Core.Entities;

/// <summary>
/// Represents a concentrated pigment colorant (e.g. Black, Oxide Red, Phthalo Blue)
/// dispensed into a paint base to produce a shade.
/// </summary>
public class Colorant
{
    /// <summary>
    /// Unique database identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Machine dispensing code (e.g. "C01", "C02", "C03"). Required and unique.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive name of the colorant (e.g. "Black", "Phthalo Blue").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cost per milliliter of the concentrated tint, stored in decimal(10,4).
    /// </summary>
    public decimal CostPerMl { get; set; }

    /// <summary>
    /// Navigation property: Formula items utilizing this colorant.
    /// </summary>
    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();

    /// <summary>
    /// Navigation property: Historical dispense job line items dispensing this colorant.
    /// </summary>
    public ICollection<DispenseJobItem> DispenseJobItems { get; set; } = new List<DispenseJobItem>();
}
