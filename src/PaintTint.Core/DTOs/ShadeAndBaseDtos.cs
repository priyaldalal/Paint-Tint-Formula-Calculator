namespace PaintTint.Core.DTOs;

/// <summary>
/// Data transfer object representing a paint base with pricing and tint capacity.
/// </summary>
public class BaseDto
{
    /// <summary>
    /// Unique database identifier of the base.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the base (Pastel, Medium, Deep).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum allowed tint percentage by volume (e.g. 2.00, 6.00, 12.00).
    /// </summary>
    public decimal MaxTintPercent { get; set; }

    /// <summary>
    /// Price per litre of the untinted base paint.
    /// </summary>
    public decimal PricePerLitre { get; set; }
}

/// <summary>
/// Data transfer object representing a concentrated colorant tint.
/// </summary>
public class ColorantDto
{
    /// <summary>
    /// Unique database identifier of the colorant.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Machine dispensing code (e.g. C01, C02).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the colorant (e.g. Black, Oxide Red).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit cost per milliliter.
    /// </summary>
    public decimal CostPerMl { get; set; }
}

/// <summary>
/// Lightweight shade summary for display in search lists and selection menus.
/// </summary>
public class ShadeSummaryDto
{
    /// <summary>
    /// Unique database identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Catalog code (e.g. OM-201).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Shade name (e.g. Ocean Mist).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hex color code for rendering UI swatches (e.g. #7FA7B5).
    /// </summary>
    public string HexColor { get; set; } = string.Empty;
}

/// <summary>
/// Formula ingredient specification for a 1-litre container.
/// </summary>
public class FormulaItemDto
{
    /// <summary>
    /// Colorant ID.
    /// </summary>
    public int ColorantId { get; set; }

    /// <summary>
    /// Colorant machine code (e.g. C03).
    /// </summary>
    public string ColorantCode { get; set; } = string.Empty;

    /// <summary>
    /// Colorant display name (e.g. Phthalo Blue).
    /// </summary>
    public string ColorantName { get; set; } = string.Empty;

    /// <summary>
    /// Required volume in milliliters for a 1-litre can.
    /// </summary>
    public decimal MlPerLitre { get; set; }

    /// <summary>
    /// Unit cost per milliliter.
    /// </summary>
    public decimal CostPerMl { get; set; }
}

/// <summary>
/// Grouped recipe formulation for a specific paint base.
/// </summary>
public class BaseFormulaDto
{
    /// <summary>
    /// Target base paint identifier.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Target base paint name (e.g. Medium).
    /// </summary>
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// List of colorant ingredients required for this base.
    /// </summary>
    public List<FormulaItemDto> Items { get; set; } = new();
}

/// <summary>
/// Detailed shade model including all supported base formulations.
/// </summary>
public class ShadeDetailDto
{
    /// <summary>
    /// Unique database identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Shade catalog code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Shade catalog name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hex color code for swatch display.
    /// </summary>
    public string HexColor { get; set; } = string.Empty;

    /// <summary>
    /// Supported formulations grouped by base paint.
    /// </summary>
    public List<BaseFormulaDto> Formulas { get; set; } = new();
}
