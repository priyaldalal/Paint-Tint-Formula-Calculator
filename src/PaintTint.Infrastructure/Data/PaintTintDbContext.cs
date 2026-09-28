using Microsoft.EntityFrameworkCore;
using PaintTint.Core.Entities;

namespace PaintTint.Infrastructure.Data;

public class PaintTintDbContext : DbContext
{
    public PaintTintDbContext(DbContextOptions<PaintTintDbContext> options) : base(options)
    {
    }

    public DbSet<Base> Bases => Set<Base>();
    public DbSet<Colorant> Colorants => Set<Colorant>();
    public DbSet<Shade> Shades => Set<Shade>();
    public DbSet<FormulaItem> FormulaItems => Set<FormulaItem>();
    public DbSet<DispenseJob> DispenseJobs => Set<DispenseJob>();
    public DbSet<DispenseJobItem> DispenseJobItems => Set<DispenseJobItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bases configuration
        modelBuilder.Entity<Base>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.MaxTintPercent).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.PricePerLitre).IsRequired().HasPrecision(10, 2);
        });

        // Colorants configuration
        modelBuilder.Entity<Colorant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CostPerMl).IsRequired().HasPrecision(10, 4);
        });

        // Shades configuration
        modelBuilder.Entity<Shade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name); // Required, indexed for search
            entity.Property(e => e.HexColor).IsRequired().HasMaxLength(7);
        });

        // FormulaItems configuration
        modelBuilder.Entity<FormulaItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MlPerLitre).IsRequired().HasPrecision(10, 4);

            entity.HasOne(e => e.Shade)
                  .WithMany(s => s.FormulaItems)
                  .HasForeignKey(e => e.ShadeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Base)
                  .WithMany(b => b.FormulaItems)
                  .HasForeignKey(e => e.BaseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Colorant)
                  .WithMany(c => c.FormulaItems)
                  .HasForeignKey(e => e.ColorantId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on (ShadeId, BaseId, ColorantId)
            entity.HasIndex(e => new { e.ShadeId, e.BaseId, e.ColorantId }).IsUnique();
        });

        // DispenseJobs configuration
        modelBuilder.Entity<DispenseJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CanSizeLitres).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.TotalColorantMl).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.TintPercent).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.TotalPrice).IsRequired().HasPrecision(12, 2);
            entity.Property(e => e.CreatedAtUtc).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Shade)
                  .WithMany(s => s.DispenseJobs)
                  .HasForeignKey(e => e.ShadeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Base)
                  .WithMany(b => b.DispenseJobs)
                  .HasForeignKey(e => e.BaseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DispenseJobItems configuration
        modelBuilder.Entity<DispenseJobItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DispensedMl).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.Cost).IsRequired().HasPrecision(10, 2);

            entity.HasOne(e => e.DispenseJob)
                  .WithMany(j => j.Items)
                  .HasForeignKey(e => e.DispenseJobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Colorant)
                  .WithMany(c => c.DispenseJobItems)
                  .HasForeignKey(e => e.ColorantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
