namespace PaintTint.Core.Entities;

/// <summary>
/// Represents a single line item of a dispensed job, recording the exact milliliters
/// dispensed and cost charged for a specific colorant, preserving audit correctness even if prices or formulas change.
/// </summary>
public class DispenseJobItem
{
    /// <summary>
    /// Unique database identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the parent <see cref="DispenseJob"/>.
    /// </summary>
    public int DispenseJobId { get; set; }

    /// <summary>
    /// Navigation property to the parent <see cref="DispenseJob"/>.
    /// </summary>
    public DispenseJob? DispenseJob { get; set; }

    /// <summary>
    /// Foreign key referencing the dispensed <see cref="Colorant"/>.
    /// </summary>
    public int ColorantId { get; set; }

    /// <summary>
    /// Navigation property to the dispensed <see cref="Colorant"/>.
    /// </summary>
    public Colorant? Colorant { get; set; }

    /// <summary>
    /// Actual volume dispensed in milliliters (scaled and rounded to nearest 0.05 ml).
    /// </summary>
    public decimal DispensedMl { get; set; }

    /// <summary>
    /// Historical calculated cost of this colorant component at the time of dispensing.
    /// </summary>
    public decimal Cost { get; set; }
}
