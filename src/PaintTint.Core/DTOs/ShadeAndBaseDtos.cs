namespace PaintTint.Core.DTOs;

public class BaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MaxTintPercent { get; set; }
    public decimal PricePerLitre { get; set; }
}

public class ColorantDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CostPerMl { get; set; }
}

public class ShadeSummaryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
}

public class FormulaItemDto
{
    public int ColorantId { get; set; }
    public string ColorantCode { get; set; } = string.Empty;
    public string ColorantName { get; set; } = string.Empty;
    public decimal MlPerLitre { get; set; }
    public decimal CostPerMl { get; set; }
}

public class BaseFormulaDto
{
    public int BaseId { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public List<FormulaItemDto> Items { get; set; } = new();
}

public class ShadeDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public List<BaseFormulaDto> Formulas { get; set; } = new();
}
