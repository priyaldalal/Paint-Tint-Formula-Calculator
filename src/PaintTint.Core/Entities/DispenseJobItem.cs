namespace PaintTint.Core.Entities;

public class DispenseJobItem
{
    public int Id { get; set; }

    public int DispenseJobId { get; set; }
    public DispenseJob? DispenseJob { get; set; }

    public int ColorantId { get; set; }
    public Colorant? Colorant { get; set; }

    public decimal DispensedMl { get; set; }
    public decimal Cost { get; set; }
}
