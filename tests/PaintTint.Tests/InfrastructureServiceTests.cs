using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PaintTint.Core.DTOs;
using PaintTint.Core.Entities;
using PaintTint.Core.Exceptions;
using PaintTint.Core.Services;
using PaintTint.Infrastructure.Data;
using PaintTint.Infrastructure.Services;
using Xunit;

namespace PaintTint.Tests;

public class InfrastructureServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PaintTintDbContext _context;
    private readonly TintCalculator _calculator;

    public InfrastructureServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PaintTintDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new PaintTintDbContext(options);
        _context.Database.EnsureCreated();

        _calculator = new TintCalculator();

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var pastel = new Base { Id = 1, Name = "Pastel", MaxTintPercent = 2.00m, PricePerLitre = 250.00m };
        var medium = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };
        var deep = new Base { Id = 3, Name = "Deep", MaxTintPercent = 12.00m, PricePerLitre = 290.00m };
        _context.Bases.AddRange(pastel, medium, deep);

        var c01 = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };
        var c03 = new Colorant { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.20m };
        _context.Colorants.AddRange(c01, c03);

        var shade = new Shade { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" };
        _context.Shades.Add(shade);

        _context.FormulaItems.AddRange(
            new FormulaItem { Id = 1, ShadeId = 1, BaseId = 2, ColorantId = 3, MlPerLitre = 4.35m },
            new FormulaItem { Id = 2, ShadeId = 1, BaseId = 2, ColorantId = 1, MlPerLitre = 1.10m }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task BaseService_GetBasesAsync_ShouldReturnAllBases()
    {
        var service = new BaseService(_context);
        var bases = await service.GetBasesAsync();

        Assert.Equal(3, bases.Count);
        Assert.Equal("Pastel", bases[0].Name);
        Assert.Equal("Medium", bases[1].Name);
        Assert.Equal("Deep", bases[2].Name);
    }

    [Fact]
    public async Task ShadeService_GetShadesAsync_ShouldFilterBySearch()
    {
        var service = new ShadeService(_context);

        var all = await service.GetShadesAsync(null);
        Assert.Single(all);

        var filtered = await service.GetShadesAsync("Mist");
        Assert.Single(filtered);

        var nonExistent = await service.GetShadesAsync("UnknownColor");
        Assert.Empty(nonExistent);
    }

    [Fact]
    public async Task ShadeService_GetShadeByIdAsync_ShouldReturnDetailsWithFormulas()
    {
        var service = new ShadeService(_context);

        var details = await service.GetShadeByIdAsync(1);
        Assert.NotNull(details);
        Assert.Equal("Ocean Mist", details.Name);
        Assert.Single(details.Formulas);
        Assert.Equal(2, details.Formulas[0].Items.Count);

        var notFound = await service.GetShadeByIdAsync(999);
        Assert.Null(notFound);
    }

    [Fact]
    public async Task DispenseService_CalculateAsync_ShouldCalculateFormulaCorrectly()
    {
        var service = new DispenseService(_context, _calculator);

        var request = new CalculateTintRequest
        {
            ShadeId = 1,
            BaseId = 2,
            CanSizeLitres = 4m
        };

        var response = await service.CalculateAsync(request);

        Assert.NotNull(response);
        Assert.True(response.IsValid);
        Assert.Equal(4m, response.CanSizeLitres);
        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task DispenseService_CalculateAsync_ShouldThrowNotFoundException_WhenMissing()
    {
        var service = new DispenseService(_context, _calculator);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CalculateAsync(new CalculateTintRequest { ShadeId = 999, BaseId = 2, CanSizeLitres = 4m }));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CalculateAsync(new CalculateTintRequest { ShadeId = 1, BaseId = 999, CanSizeLitres = 4m }));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CalculateAsync(new CalculateTintRequest { ShadeId = 1, BaseId = 1, CanSizeLitres = 4m }));
    }

    [Fact]
    public async Task DispenseService_CreateDispenseJobAsync_ShouldPersistJobAndItems()
    {
        var service = new DispenseService(_context, _calculator);

        var request = new CreateDispenseJobRequest
        {
            ShadeId = 1,
            BaseId = 2,
            CanSizeLitres = 4m
        };

        var job = await service.CreateDispenseJobAsync(request);

        Assert.NotNull(job);
        Assert.True(job.Id > 0);
        Assert.Equal("Ocean Mist", job.ShadeName);
        Assert.Equal(2, job.Items.Count);

        var recent = await service.GetRecentJobsAsync(10);
        Assert.Single(recent);

        var latest = await service.GetLatestJobAsync();
        Assert.NotNull(latest);
        Assert.Equal(job.Id, latest.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
