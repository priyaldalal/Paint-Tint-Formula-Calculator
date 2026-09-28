namespace PaintTint.Core.Entities;

public class DispenseJob
{
    public int Id { get; set; }

    public int ShadeId { get; set; }
    public Shade? Shade { get; set; }

    public int BaseId { get; set; }
    public Base? Base { get; set; }

    public decimal CanSizeLitres { get; set; }
    public decimal TotalColorantMl { get; set; }
    public decimal TintPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DispenseJobItem> Items { get; set; } = new List<DispenseJobItem>();
}
