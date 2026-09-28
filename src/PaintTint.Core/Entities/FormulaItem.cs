namespace PaintTint.Core.Entities;

/// <summary>
/// Represents a single ingredient entry in a shade's formulation recipe for a specific base paint,
/// indicating the volume of colorant in milliliters required for a 1-litre container.
/// </summary>
public class FormulaItem
{
    /// <summary>
    /// Unique database identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the parent <see cref="Shade"/>.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Navigation property to the parent <see cref="Shade"/>.
    /// </summary>
    public Shade? Shade { get; set; }

    /// <summary>
    /// Foreign key referencing the compatible <see cref="Base"/>.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Navigation property to the compatible <see cref="Base"/>.
    /// </summary>
    public Base? Base { get; set; }

    /// <summary>
    /// Foreign key referencing the required <see cref="Colorant"/>.
    /// </summary>
    public int ColorantId { get; set; }

    /// <summary>
    /// Navigation property to the required <see cref="Colorant"/>.
    /// </summary>
    public Colorant? Colorant { get; set; }

    /// <summary>
    /// Volume of colorant in milliliters required per 1 Litre of base paint. Stored in decimal(10,4).
    /// </summary>
    public decimal MlPerLitre { get; set; }
}
