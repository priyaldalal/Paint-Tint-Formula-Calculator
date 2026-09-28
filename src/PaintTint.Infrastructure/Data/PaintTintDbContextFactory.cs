using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaintTint.Infrastructure.Data;

public class PaintTintDbContextFactory : IDesignTimeDbContextFactory<PaintTintDbContext>
{
    public PaintTintDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaintTintDbContext>();
        optionsBuilder.UseSqlServer("Server=PRIYAL\\SQLEXPRESS;Database=PaintTintDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;");

        return new PaintTintDbContext(optionsBuilder.Options);
    }
}
