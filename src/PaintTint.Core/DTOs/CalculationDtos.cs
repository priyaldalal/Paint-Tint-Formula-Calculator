namespace PaintTint.Core.DTOs;

/// <summary>
/// Request payload for calculating scaled tint formulation and pricing.
/// </summary>
public class CalculateTintRequest
{
    /// <summary>
    /// Gets or sets the target Shade identifier.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Gets or sets the target Base paint identifier.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Gets or sets the target paint can size in litres (e.g. 1, 4, 10, 20).
    /// </summary>
    public decimal CanSizeLitres { get; set; }
}

/// <summary>
/// Details of an individual colorant scaled for a specific can size.
/// </summary>
public class CalculatedColorantItemDto
{
    /// <summary>
    /// Gets or sets the unique colorant identifier.
    /// </summary>
    public int ColorantId { get; set; }

    /// <summary>
    /// Gets or sets the colorant code (e.g., "WH", "BK").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive colorant name (e.g., "Titanium White").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the master formula volume in millilitres per litre (ml/L).
    /// </summary>
    public decimal MlPerLitre { get; set; }

    /// <summary>
    /// Gets or sets the calculated volume rounded to the nearest dispenser resolution (0.05 ml).
    /// </summary>
    public decimal ScaledMl { get; set; }

    /// <summary>
    /// Gets or sets the unit cost per millilitre (₹/ml).
    /// </summary>
    public decimal CostPerMl { get; set; }

    /// <summary>
    /// Gets or sets the total cost of this colorant component for the specified can size.
    /// </summary>
    public decimal Cost { get; set; }
}

/// <summary>
/// Complete calculation response containing scaled volumes, validation status, and final price breakdown.
/// </summary>
public class CalculateTintResponse
{
    /// <summary>
    /// Gets or sets the shade identifier.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Gets or sets the shade code.
    /// </summary>
    public string ShadeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shade name.
    /// </summary>
    public string ShadeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hex color code for UI preview.
    /// </summary>
    public string HexColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected base identifier.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Gets or sets the base paint name (e.g., Pastel, Medium, Deep).
    /// </summary>
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum allowable tint percentage for the base.
    /// </summary>
    public decimal MaxTintPercent { get; set; }

    /// <summary>
    /// Gets or sets the selected can size in litres.
    /// </summary>
    public decimal CanSizeLitres { get; set; }

    /// <summary>
    /// Gets or sets the base paint unit price per litre.
    /// </summary>
    public decimal BasePricePerLitre { get; set; }

    /// <summary>
    /// Gets or sets the base paint cost (BasePricePerLitre * CanSizeLitres).
    /// </summary>
    public decimal BaseCost { get; set; }

    /// <summary>
    /// Gets or sets the sum of all dispensed colorant costs.
    /// </summary>
    public decimal ColorantsCost { get; set; }

    /// <summary>
    /// Gets or sets the total volume of all colorants in millilitres.
    /// </summary>
    public decimal TotalColorantMl { get; set; }

    /// <summary>
    /// Gets or sets the actual tint percentage = (TotalColorantMl / (CanSizeLitres * 1000)) * 100.
    /// </summary>
    public decimal TintPercent { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed total colorant volume in millilitres for this can size and base.
    /// </summary>
    public decimal MaxAllowedColorantMl { get; set; }

    /// <summary>
    /// Gets or sets the final calculated retail price: BaseCost + ColorantsCost.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tint formulation complies with the base maximum tint percentage.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets a descriptive validation message if the formulation exceeds maximum limits.
    /// </summary>
    public string? ValidationError { get; set; }

    /// <summary>
    /// Gets or sets the detailed breakdown of each scaled colorant item.
    /// </summary>
    public List<CalculatedColorantItemDto> Items { get; set; } = new();
}

/// <summary>
/// Request payload to execute and record a dispense operation.
/// </summary>
public class CreateDispenseJobRequest
{
    /// <summary>
    /// Gets or sets the shade identifier to dispense.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Gets or sets the base paint identifier.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Gets or sets the can volume in litres.
    /// </summary>
    public decimal CanSizeLitres { get; set; }
}

/// <summary>
/// Response item representing an individual colorant dispensed in a historical job.
/// </summary>
public class DispenseJobItemResponse
{
    /// <summary>
    /// Gets or sets the primary key of the dispense job line item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the colorant identifier.
    /// </summary>
    public int ColorantId { get; set; }

    /// <summary>
    /// Gets or sets the colorant code.
    /// </summary>
    public string ColorantCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the colorant name.
    /// </summary>
    public string ColorantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actual volume dispensed in millilitres.
    /// </summary>
    public decimal DispensedMl { get; set; }

    /// <summary>
    /// Gets or sets the cost of this dispensed colorant.
    /// </summary>
    public decimal Cost { get; set; }
}

/// <summary>
/// Response model representing a completed dispense job record.
/// </summary>
public class DispenseJobResponse
{
    /// <summary>
    /// Gets or sets the unique dispense job identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the shade identifier.
    /// </summary>
    public int ShadeId { get; set; }

    /// <summary>
    /// Gets or sets the shade code.
    /// </summary>
    public string ShadeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shade name.
    /// </summary>
    public string ShadeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base identifier.
    /// </summary>
    public int BaseId { get; set; }

    /// <summary>
    /// Gets or sets the base name.
    /// </summary>
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the can size in litres.
    /// </summary>
    public decimal CanSizeLitres { get; set; }

    /// <summary>
    /// Gets or sets the total dispensed colorant volume in millilitres.
    /// </summary>
    public decimal TotalColorantMl { get; set; }

    /// <summary>
    /// Gets or sets the tint percentage relative to can volume.
    /// </summary>
    public decimal TintPercent { get; set; }

    /// <summary>
    /// Gets or sets the total retail price charged.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the dispense job was recorded.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the colorant components that were dispensed.
    /// </summary>
    public List<DispenseJobItemResponse> Items { get; set; } = new();
}
