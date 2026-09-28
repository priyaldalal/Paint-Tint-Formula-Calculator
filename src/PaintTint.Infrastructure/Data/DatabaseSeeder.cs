using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PaintTint.Core.Entities;

namespace PaintTint.Infrastructure.Data;

public class SeedDataFormat
{
    [JsonPropertyName("bases")]
    public List<SeedBase> Bases { get; set; } = new();

    [JsonPropertyName("colorants")]
    public List<SeedColorant> Colorants { get; set; } = new();

    [JsonPropertyName("shades")]
    public List<SeedShade> Shades { get; set; } = new();
}

public class SeedBase
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("maxTintPercent")]
    public decimal MaxTintPercent { get; set; }

    [JsonPropertyName("pricePerLitre")]
    public decimal PricePerLitre { get; set; }
}

public class SeedColorant
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("costPerMl")]
    public decimal CostPerMl { get; set; }
}

public class SeedShade
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("hex")]
    public string Hex { get; set; } = string.Empty;

    [JsonPropertyName("formulas")]
    public List<SeedShadeFormula> Formulas { get; set; } = new();
}

public class SeedShadeFormula
{
    [JsonPropertyName("baseId")]
    public int BaseId { get; set; }

    [JsonPropertyName("items")]
    public List<SeedFormulaItem> Items { get; set; } = new();
}

public class SeedFormulaItem
{
    [JsonPropertyName("colorantId")]
    public int ColorantId { get; set; }

    [JsonPropertyName("mlPerLitre")]
    public decimal MlPerLitre { get; set; }
}

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PaintTintDbContext context, string? seedJsonPath = null)
    {
        string jsonContent = string.Empty;

        // Try candidate locations for seed.json
        var pathsToTry = new List<string>();
        if (!string.IsNullOrWhiteSpace(seedJsonPath))
            pathsToTry.Add(seedJsonPath);

        pathsToTry.Add(Path.Combine(AppContext.BaseDirectory, "seed.json"));
        pathsToTry.Add(Path.Combine(AppContext.BaseDirectory, "Data", "seed.json"));
        pathsToTry.Add(Path.Combine(Directory.GetCurrentDirectory(), "data", "seed.json"));
        pathsToTry.Add(Path.Combine(Directory.GetCurrentDirectory(), "seed.json"));
        pathsToTry.Add(Path.Combine(Directory.GetCurrentDirectory(), "..", "PaintTint.Infrastructure", "Data", "seed.json"));

        foreach (var p in pathsToTry)
        {
            if (File.Exists(p))
            {
                jsonContent = await File.ReadAllTextAsync(p);
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            throw new FileNotFoundException("Could not find seed.json in any expected paths.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var seedData = JsonSerializer.Deserialize<SeedDataFormat>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize seed.json.");

        // Check if database already has data
        if (await context.Bases.AnyAsync())
        {
            await SyncFormulasAsync(context, seedData);
            return;
        }

        // 1. Seed Bases
        foreach (var b in seedData.Bases)
        {
            context.Bases.Add(new Base
            {
                Name = b.Name,
                MaxTintPercent = b.MaxTintPercent,
                PricePerLitre = b.PricePerLitre
            });
        }
        await context.SaveChangesAsync();

        var basesByName = await context.Bases.ToDictionaryAsync(b => b.Name, b => b.Id);
        // Map seed base ID to inserted base ID (seed ID 1 -> Pastel, 2 -> Medium, 3 -> Deep)
        var seedBaseIdToDbId = seedData.Bases.ToDictionary(
            b => b.Id,
            b => basesByName[b.Name]
        );

        // 2. Seed Colorants
        foreach (var c in seedData.Colorants)
        {
            context.Colorants.Add(new Colorant
            {
                Code = c.Code,
                Name = c.Name,
                CostPerMl = c.CostPerMl
            });
        }
        await context.SaveChangesAsync();

        var colorantsByCode = await context.Colorants.ToDictionaryAsync(c => c.Code, c => c.Id);
        var seedColorantIdToDbId = seedData.Colorants.ToDictionary(
            c => c.Id,
            c => colorantsByCode[c.Code]
        );

        // 3. Seed Shades and FormulaItems
        foreach (var s in seedData.Shades)
        {
            var shade = new Shade
            {
                Code = s.Code,
                Name = s.Name,
                HexColor = s.Hex
            };

            context.Shades.Add(shade);
            await context.SaveChangesAsync();

            foreach (var formula in s.Formulas)
            {
                int baseDbId = seedBaseIdToDbId[formula.BaseId];

                foreach (var item in formula.Items)
                {
                    int colorantDbId = seedColorantIdToDbId[item.ColorantId];

                    context.FormulaItems.Add(new FormulaItem
                    {
                        ShadeId = shade.Id,
                        BaseId = baseDbId,
                        ColorantId = colorantDbId,
                        MlPerLitre = item.MlPerLitre
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SyncFormulasAsync(PaintTintDbContext context, SeedDataFormat seedData)
    {
        var shadesByCode = await context.Shades.ToDictionaryAsync(s => s.Code, s => s.Id);
        var basesByName = await context.Bases.ToDictionaryAsync(b => b.Name, b => b.Id);
        var seedBaseIdToDbId = seedData.Bases.ToDictionary(b => b.Id, b => basesByName[b.Name]);
        var colorantsByCode = await context.Colorants.ToDictionaryAsync(c => c.Code, c => c.Id);
        var seedColorantIdToDbId = seedData.Colorants.ToDictionary(c => c.Id, c => colorantsByCode[c.Code]);

        var existingKeysList = await context.FormulaItems
            .Select(f => f.ShadeId + "_" + f.BaseId + "_" + f.ColorantId)
            .ToListAsync();
        var existingFormulaKeys = existingKeysList.ToHashSet();

        bool added = false;
        foreach (var s in seedData.Shades)
        {
            if (!shadesByCode.TryGetValue(s.Code, out int shadeId))
                continue;

            foreach (var formula in s.Formulas)
            {
                if (!seedBaseIdToDbId.TryGetValue(formula.BaseId, out int baseDbId))
                    continue;

                foreach (var item in formula.Items)
                {
                    if (!seedColorantIdToDbId.TryGetValue(item.ColorantId, out int colorantDbId))
                        continue;

                    string key = $"{shadeId}_{baseDbId}_{colorantDbId}";
                    if (!existingFormulaKeys.Contains(key))
                    {
                        context.FormulaItems.Add(new FormulaItem
                        {
                            ShadeId = shadeId,
                            BaseId = baseDbId,
                            ColorantId = colorantDbId,
                            MlPerLitre = item.MlPerLitre
                        });
                        existingFormulaKeys.Add(key);
                        added = true;
                    }
                }
            }
        }

        if (added)
        {
            await context.SaveChangesAsync();
        }
    }
}
