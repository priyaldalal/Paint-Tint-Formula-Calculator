namespace PaintTint.Core.Entities;

/// <summary>
/// Represents a paint base product (such as Pastel, Medium, or Deep) to which colorants are added.
/// Defines maximum allowable tint percentage and base price per litre.
/// </summary>
public class Base
{
    /// <summary>
    /// Unique database identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the base paint (e.g. "Pastel", "Medium", "Deep"). Required and unique.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum allowable tint percentage by volume (e.g. 2.00 for Pastel, 6.00 for Medium, 12.00 for Deep).
    /// </summary>
    public decimal MaxTintPercent { get; set; }

    /// <summary>
    /// Cost of the untinted base paint per litre.
    /// </summary>
    public decimal PricePerLitre { get; set; }

    /// <summary>
    /// Navigation property: Formula items defined for this base.
    /// </summary>
    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();

    /// <summary>
    /// Navigation property: Historical dispense jobs tinted using this base.
    /// </summary>
    public ICollection<DispenseJob> DispenseJobs { get; set; } = new List<DispenseJob>();
}
