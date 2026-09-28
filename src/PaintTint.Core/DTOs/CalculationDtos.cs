namespace PaintTint.Core.DTOs;

public class CalculateTintRequest
{
    public int ShadeId { get; set; }
    public int BaseId { get; set; }
    public decimal CanSizeLitres { get; set; }
}

public class CalculatedColorantItemDto
{
    public int ColorantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MlPerLitre { get; set; }
    public decimal ScaledMl { get; set; }
    public decimal CostPerMl { get; set; }
    public decimal Cost { get; set; }
}

public class CalculateTintResponse
{
    public int ShadeId { get; set; }
    public string ShadeCode { get; set; } = string.Empty;
    public string ShadeName { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;

    public int BaseId { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public decimal MaxTintPercent { get; set; }
    public decimal CanSizeLitres { get; set; }

    public decimal BasePricePerLitre { get; set; }
    public decimal BaseCost { get; set; }
    public decimal ColorantsCost { get; set; }
    public decimal TotalColorantMl { get; set; }
    public decimal TintPercent { get; set; }
    public decimal MaxAllowedColorantMl { get; set; }
    public decimal TotalPrice { get; set; }

    public bool IsValid { get; set; }
    public string? ValidationError { get; set; }

    public List<CalculatedColorantItemDto> Items { get; set; } = new();
}

public class CreateDispenseJobRequest
{
    public int ShadeId { get; set; }
    public int BaseId { get; set; }
    public decimal CanSizeLitres { get; set; }
}

public class DispenseJobItemResponse
{
    public int Id { get; set; }
    public int ColorantId { get; set; }
    public string ColorantCode { get; set; } = string.Empty;
    public string ColorantName { get; set; } = string.Empty;
    public decimal DispensedMl { get; set; }
    public decimal Cost { get; set; }
}

public class DispenseJobResponse
{
    public int Id { get; set; }
    public int ShadeId { get; set; }
    public string ShadeCode { get; set; } = string.Empty;
    public string ShadeName { get; set; } = string.Empty;
    public int BaseId { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public decimal CanSizeLitres { get; set; }
    public decimal TotalColorantMl { get; set; }
    public decimal TintPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<DispenseJobItemResponse> Items { get; set; } = new();
}
