using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaintTint.Infrastructure.Data;

public class PaintTintDbContextFactory : IDesignTimeDbContextFactory<PaintTintDbContext>
{
    public PaintTintDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaintTintDbContext>();
        optionsBuilder.UseSqlite("Data Source=painttint.db");

        return new PaintTintDbContext(optionsBuilder.Options);
    }
}
