namespace PaintTint.Core.Entities;

/// <summary>
/// Represents an immutable historical record of a physical paint can dispensing event,
/// capturing the exact shade, base, can size, total colorant, tint percentage, and price at the time of dispensing.
/// </summary>
public class DispenseJob
{
    /// <summary>
    /// Unique database job identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key referencing the dispensed <see cref="Shade"/>.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Navigation property to the dispensed <see cref="Shade"/>.
    /// </summary>
    public Shade? Shade { get; set; }

    /// <summary>
    /// Foreign key referencing the used <see cref="Base"/>.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Navigation property to the used <see cref="Base"/>.
    /// </summary>
    public Base? Base { get; set; }

    /// <summary>
    /// Container size in litres (e.g. 1.00, 4.00, 10.00, 20.00).
    /// </summary>
    public decimal CanSizeLitres { get; set; }

    /// <summary>
    /// Total volume of all colorants dispensed in milliliters.
    /// </summary>
    public decimal TotalColorantMl { get; set; }

    /// <summary>
    /// Resulting tint percentage of the total can volume (TotalColorantMl / CanVolumeMl * 100).
    /// </summary>
    public decimal TintPercent { get; set; }

    /// <summary>
    /// Final price charged to customer in currency units, stored in decimal(12,2).
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// UTC timestamp when the job was executed.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: Itemized records of each colorant dispensed and historical cost.
    /// </summary>
    public ICollection<DispenseJobItem> Items { get; set; } = new List<DispenseJobItem>();
}
