namespace PaintTint.Core.Entities;

/// <summary>
/// Represents a named catalog shade color (e.g. "Ocean Mist", "Terracotta")
/// associated with specific colorant formulation recipes.
/// </summary>
public class Shade
{
    /// <summary>
    /// Unique database identifier (Primary Key, Identity).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique catalog identifier code (e.g. "OM-201", "TR-115"). Required and unique.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable shade name (e.g. "Ocean Mist"). Required, indexed for search.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Visual hex color code for UI swatches and display (e.g. "#7FA7B5"). Required.
    /// </summary>
    public string HexColor { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property: Formula items defining colorants per base for this shade.
    /// </summary>
    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();

    /// <summary>
    /// Navigation property: Past dispense jobs completed for this shade.
    /// </summary>
    public ICollection<DispenseJob> DispenseJobs { get; set; } = new List<DispenseJob>();
}
