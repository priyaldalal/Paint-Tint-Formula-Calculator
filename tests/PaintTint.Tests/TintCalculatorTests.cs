using PaintTint.Core.DTOs;
using PaintTint.Core.Entities;
using PaintTint.Core.Exceptions;
using PaintTint.Core.Services;
using Xunit;

namespace PaintTint.Tests;

public class TintCalculatorTests
{
    private readonly TintCalculator _calculator = new();

    [Theory]
    [InlineData(0.00, 0.00)]
    [InlineData(0.02, 0.00)]
    [InlineData(0.024, 0.00)]
    [InlineData(0.025, 0.05)]
    [InlineData(0.03, 0.05)]
    [InlineData(0.07, 0.05)]
    [InlineData(0.075, 0.10)]
    [InlineData(0.08, 0.10)]
    [InlineData(4.35, 4.35)]
    [InlineData(12.80, 12.80)]
    [InlineData(17.40, 17.40)]
    [InlineData(1.123, 1.10)]
    [InlineData(1.127, 1.15)]
    public void RoundToDispenserUnit_ShouldRoundToNearestZeroPointZeroFive(decimal input, decimal expected)
    {
        // Act
        decimal result = _calculator.RoundToDispenserUnit(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_ShouldScaleColorantsLinearly_ForDifferentCanSizes()
    {
        // Arrange
        var shade = new Shade { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" };
        var basePaint = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };
        var colorant = new Colorant { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.20m };

        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 1, BaseId = 2, ColorantId = 3, Colorant = colorant, MlPerLitre = 4.35m }
        };

        // Act & Assert 1L
        var result1L = _calculator.Calculate(shade, basePaint, 1m, formulaItems);
        Assert.Equal(4.35m, result1L.Items[0].ScaledMl);

        // Act & Assert 4L
        var result4L = _calculator.Calculate(shade, basePaint, 4m, formulaItems);
        Assert.Equal(17.40m, result4L.Items[0].ScaledMl); // 4.35 * 4 = 17.40

        // Act & Assert 10L
        var result10L = _calculator.Calculate(shade, basePaint, 10m, formulaItems);
        Assert.Equal(43.50m, result10L.Items[0].ScaledMl); // 4.35 * 10 = 43.50

        // Act & Assert 20L
        var result20L = _calculator.Calculate(shade, basePaint, 20m, formulaItems);
        Assert.Equal(87.00m, result20L.Items[0].ScaledMl); // 4.35 * 20 = 87.00
    }

    [Fact]
    public void Calculate_WorkedExampleFromSpecification_MatchesExactValues()
    {
        // Specification worked example:
        // Shade needs 3.2 ml Black per litre in Medium base.
        // For a 4L can: 3.2 * 4 = 12.8 ml.
        // Total colorant must be at most 6% of 4,000 ml, which is 240 ml, so the request is valid.
        var shade = new Shade { Id = 99, Code = "EX-01", Name = "Example Gray", HexColor = "#888888" };
        var basePaint = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };
        var black = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };

        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 99, BaseId = 2, ColorantId = 1, Colorant = black, MlPerLitre = 3.20m }
        };

        // Act
        var result = _calculator.Calculate(shade, basePaint, 4m, formulaItems);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ValidationError);
        Assert.Equal(12.80m, result.TotalColorantMl);
        Assert.Equal(240.00m, result.MaxAllowedColorantMl); // 6% of 4,000 ml
        Assert.Equal(0.32m, result.TintPercent); // 12.8 / 4000 = 0.0032 = 0.32%
        
        // Base cost = 270 * 4 = 1080.00
        // Colorant cost = 12.8 * 0.80 = 10.24
        // Total price = 1080.00 + 10.24 = 1090.24
        Assert.Equal(1080.00m, result.BaseCost);
        Assert.Equal(10.24m, result.ColorantsCost);
        Assert.Equal(1090.24m, result.TotalPrice);
    }

    [Fact]
    public void Calculate_MockupSpecificationExample_MatchesUiScreenshotValues()
    {
        // From Page 4 mockup screenshot:
        // Shade: Ocean Mist (OM-201), Base: Medium (270/L), Can: 4L
        // Phthalo Blue: 4.35 ml/L -> 4L: 17.40 ml, Cost: 20.88
        // Black: 1.10 ml/L -> 4L: 4.40 ml, Cost: 3.52
        // Total colorant: 21.80 ml
        // Tint used: 0.55% of 6%
        // Price: ₹1,104.40
        var shade = new Shade { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" };
        var basePaint = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };
        var blue = new Colorant { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.20m };
        var black = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };

        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 1, BaseId = 2, ColorantId = 3, Colorant = blue, MlPerLitre = 4.35m },
            new() { Id = 2, ShadeId = 1, BaseId = 2, ColorantId = 1, Colorant = black, MlPerLitre = 1.10m }
        };

        // Act
        var result = _calculator.Calculate(shade, basePaint, 4m, formulaItems);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ValidationError);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(17.40m, result.Items[0].ScaledMl);
        Assert.Equal(20.88m, result.Items[0].Cost);
        Assert.Equal(4.40m, result.Items[1].ScaledMl);
        Assert.Equal(3.52m, result.Items[1].Cost);
        Assert.Equal(21.80m, result.TotalColorantMl);
        Assert.Equal(0.55m, result.TintPercent); // 21.80 / 4000 = 0.00545 -> rounds to 0.55%
        Assert.Equal(1080.00m, result.BaseCost);
        Assert.Equal(24.40m, result.ColorantsCost);
        Assert.Equal(1104.40m, result.TotalPrice);
    }

    [Fact]
    public void Calculate_ShouldFlagError_WhenTintExceedsMaximumBasePercentage()
    {
        // Pastel base allows only 2% max tint (20 ml per 1L, or 80 ml per 4L).
        // Let's test a shade formula requiring 25 ml/L in Pastel base -> 100 ml for 4L can.
        // 100 ml > 80 ml (2.5% > 2%), so it must fail validation!
        var shade = new Shade { Id = 3, Code = "HVY-01", Name = "Heavy Blue", HexColor = "#112233" };
        var pastelBase = new Base { Id = 1, Name = "Pastel", MaxTintPercent = 2.00m, PricePerLitre = 250.00m };
        var blue = new Colorant { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.20m };

        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 3, BaseId = 1, ColorantId = 3, Colorant = blue, MlPerLitre = 25.00m }
        };

        // Act
        var result = _calculator.Calculate(shade, pastelBase, 4m, formulaItems);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ValidationError);
        Assert.Contains("exceeds 2% limit for Pastel base", result.ValidationError);
        Assert.Equal(100.00m, result.TotalColorantMl);
        Assert.Equal(80.00m, result.MaxAllowedColorantMl);
        Assert.Equal(2.50m, result.TintPercent);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenCanSizeIsInvalid()
    {
        // Can sizes must be 1, 4, 10 or 20
        var shade = new Shade { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" };
        var basePaint = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };
        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 1, BaseId = 2, ColorantId = 1, Colorant = new Colorant { Code = "C01", CostPerMl = 1 }, MlPerLitre = 1m }
        };

        // Act & Assert
        var ex = Assert.Throws<TintValidationException>(() => _calculator.Calculate(shade, basePaint, 5m, formulaItems));
        Assert.Contains("Invalid can size: 5L", ex.Message);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenFormulaItemsAreEmpty()
    {
        var shade = new Shade { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" };
        var basePaint = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m };

        // Act & Assert
        var ex = Assert.Throws<TintValidationException>(() => _calculator.Calculate(shade, basePaint, 4m, Enumerable.Empty<FormulaItem>()));
        Assert.Contains("No formula items found", ex.Message);
    }

    [Fact]
    public void Calculate_ShouldCalculatePriceAccurately_ForHighVolumeCan()
    {
        // 20L can with Deep base (290/L) -> Base cost = 5800.00
        // Terracotta: 38.50 ml/L red, 22.20 ml/L yellow, 2.05 ml/L black
        // 20L:
        // Red: 38.50 * 20 = 770.00 ml * 0.90 = 693.00
        // Yellow: 22.20 * 20 = 444.00 ml * 0.85 = 377.40
        // Black: 2.05 * 20 = 41.00 ml * 0.80 = 32.80
        // Colorants total = 693.00 + 377.40 + 32.80 = 1103.20
        // Total price = 5800.00 + 1103.20 = 6903.20
        // Total ml = 770 + 444 + 41 = 1255.00 ml
        // Can volume = 20,000 ml
        // Max allowed for Deep (12%) = 2,400 ml
        // Tint % = 1255 / 20000 = 6.275% -> 6.28% <= 12%, valid!
        var shade = new Shade { Id = 2, Code = "TR-115", Name = "Terracotta", HexColor = "#C4663F" };
        var deepBase = new Base { Id = 3, Name = "Deep", MaxTintPercent = 12.00m, PricePerLitre = 290.00m };
        var red = new Colorant { Id = 2, Code = "C02", Name = "Oxide Red", CostPerMl = 0.90m };
        var yellow = new Colorant { Id = 4, Code = "C04", Name = "Yellow Oxide", CostPerMl = 0.85m };
        var black = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };

        var formulaItems = new List<FormulaItem>
        {
            new() { Id = 1, ShadeId = 2, BaseId = 3, ColorantId = 2, Colorant = red, MlPerLitre = 38.50m },
            new() { Id = 2, ShadeId = 2, BaseId = 3, ColorantId = 4, Colorant = yellow, MlPerLitre = 22.20m },
            new() { Id = 3, ShadeId = 2, BaseId = 3, ColorantId = 1, Colorant = black, MlPerLitre = 2.05m }
        };

        // Act
        var result = _calculator.Calculate(shade, deepBase, 20m, formulaItems);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1255.00m, result.TotalColorantMl);
        Assert.Equal(6.28m, result.TintPercent);
        Assert.Equal(5800.00m, result.BaseCost);
        Assert.Equal(1103.20m, result.ColorantsCost);
        Assert.Equal(6903.20m, result.TotalPrice);
    }
}
