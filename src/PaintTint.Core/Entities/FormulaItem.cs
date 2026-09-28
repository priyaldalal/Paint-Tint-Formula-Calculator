namespace PaintTint.Core.Entities;

public class FormulaItem
{
    public int Id { get; set; }
    public int ShadeId { get; set; }
    public Shade? Shade { get; set; }

    public int BaseId { get; set; }
    public Base? Base { get; set; }

    public int ColorantId { get; set; }
    public Colorant? Colorant { get; set; }

    public decimal MlPerLitre { get; set; }
}
