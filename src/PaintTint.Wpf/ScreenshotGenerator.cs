using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PaintTint.Core.DTOs;
using PaintTint.Wpf.Interfaces;
using PaintTint.Wpf.Services;
using PaintTint.Wpf.ViewModels;

namespace PaintTint.Wpf;

public static class ScreenshotGenerator
{
    public static void Generate()
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshot_generator.log");
        try
        {
            File.AppendAllText(logPath, $"Starting ScreenshotGenerator at {DateTime.Now}\n");
            string docsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "docs", "screenshots"));
            File.AppendAllText(logPath, $"Docs dir target: {docsDir}\n");
            if (!Directory.Exists(docsDir))
            {
                Directory.CreateDirectory(docsDir);
            }

            var mockClient = new MockScreenshotApiClient();

            // 1. Capture Main Calculation Screen
            {
                var vm = new MainViewModel(mockClient);
                vm.InitializeAsync().GetAwaiter().GetResult();
                vm.CalculateTintAsync().GetAwaiter().GetResult();
                vm.IsHistoryOpen = false;

                var window = new MainWindow(vm)
                {
                    Width = 1320,
                    Height = 780
                };
                window.Measure(new Size(1320, 780));
                window.Arrange(new Rect(0, 0, 1320, 780));
                window.UpdateLayout();

                SaveWindowToPng(window, Path.Combine(docsDir, "app_main_screen.png"));
            }

            // 2. Capture Validation Error Screen (Pastel limit exceeded)
            {
                var vm = new MainViewModel(mockClient);
                vm.InitializeAsync().GetAwaiter().GetResult();
                vm.CalculateTintAsync().GetAwaiter().GetResult();
                vm.SelectedBase = vm.Bases.FirstOrDefault(b => b.Name == "Pastel");
                vm.TotalColorantMl = 100.00m;
                vm.TintPercent = 2.50m;
                vm.MaxTintPercent = 2.00m;
                vm.TintProgressValue = 100.0;
                vm.TintProgressText = "2.50% of 2.00%";
                vm.IsValid = false;
                vm.InlineErrorMessage = "Tint of 100.00 ml (2.50%) exceeds 2% limit for Pastel base (maximum allowed: 80.00 ml).";
                vm.HasInlineError = true;
                vm.IsHistoryOpen = false;
                vm.DispenseCommand.NotifyCanExecuteChanged();

                var window = new MainWindow(vm)
                {
                    Width = 1320,
                    Height = 780
                };
                window.Measure(new Size(1320, 780));
                window.Arrange(new Rect(0, 0, 1320, 780));
                window.UpdateLayout();

                SaveWindowToPng(window, Path.Combine(docsDir, "app_validation_error.png"));
            }

            // 3. Capture History Modal Screen
            {
                var vm = new MainViewModel(mockClient);
                vm.InitializeAsync().GetAwaiter().GetResult();
                vm.CalculateTintAsync().GetAwaiter().GetResult();
                vm.IsHistoryOpen = true;
                vm.RecentJobs = new ObservableCollection<DispenseJobResponse>(mockClient.GetSampleJobs());

                var window = new MainWindow(vm)
                {
                    Width = 1320,
                    Height = 780
                };
                window.Measure(new Size(1320, 780));
                window.Arrange(new Rect(0, 0, 1320, 780));
                window.UpdateLayout();

                SaveWindowToPng(window, Path.Combine(docsDir, "app_dispense_history.png"));
            }

            File.AppendAllText(logPath, "All screenshots saved successfully!\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(logPath, $"Error: {ex}\n");
        }
    }

    private static void SaveWindowToPng(MainWindow window, string filePath)
    {
        int width = 1320;
        int height = 780;

        var content = (FrameworkElement)window.Content;
        var dc = window.DataContext;
        window.Content = null; // Detach from window to allow independent visual tree layout
        content.DataContext = dc;

        content.Width = width;
        content.Height = height;
        content.Measure(new Size(width, height));
        content.Arrange(new Rect(0, 0, width, height));
        content.UpdateLayout();

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(content);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        using var fileStream = File.Create(filePath);
        encoder.Save(fileStream);
    }
}

public class MockScreenshotApiClient : ITintApiClient
{
    public Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<BaseDto>
        {
            new() { Id = 1, Name = "Pastel", MaxTintPercent = 2.00m, PricePerLitre = 250.00m },
            new() { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m },
            new() { Id = 3, Name = "Deep", MaxTintPercent = 12.00m, PricePerLitre = 290.00m }
        });
    }

    public Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<ShadeSummaryDto>
        {
            new() { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" },
            new() { Id = 2, Code = "TR-115", Name = "Terracotta", HexColor = "#C4663F" },
            new() { Id = 3, Code = "SW-101", Name = "Soft White", HexColor = "#F5F5F0" },
            new() { Id = 4, Code = "SG-205", Name = "Sage Green", HexColor = "#8A9A86" },
            new() { Id = 5, Code = "MN-310", Name = "Midnight Navy", HexColor = "#1E2D4A" },
            new() { Id = 6, Code = "CR-220", Name = "Coral Rose", HexColor = "#E07A5F" }
        });
    }

    public Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<ShadeDetailDto?>(null);

    public Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CalculateTintResponse
        {
            ShadeId = 1,
            ShadeCode = "OM-201",
            ShadeName = "Ocean Mist",
            HexColor = "#7FA7B5",
            BaseId = 2,
            BaseName = "Medium",
            MaxTintPercent = 6.00m,
            CanSizeLitres = 4m,
            BasePricePerLitre = 270.00m,
            BaseCost = 1080.00m,
            ColorantsCost = 24.40m,
            TotalColorantMl = 21.80m,
            TintPercent = 0.55m,
            MaxAllowedColorantMl = 240.00m,
            TotalPrice = 1104.40m,
            IsValid = true,
            Items = new List<CalculatedColorantItemDto>
            {
                new() { ColorantId = 3, Code = "C03", Name = "Phthalo Blue", MlPerLitre = 4.35m, ScaledMl = 17.40m, CostPerMl = 1.20m, Cost = 20.88m },
                new() { ColorantId = 1, Code = "C01", Name = "Black", MlPerLitre = 1.10m, ScaledMl = 4.40m, CostPerMl = 0.80m, Cost = 3.52m }
            }
        });
    }

    public Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DispenseJobResponse
        {
            Id = 1042,
            ShadeId = 1,
            ShadeCode = "OM-201",
            ShadeName = "Ocean Mist",
            BaseId = 2,
            BaseName = "Medium",
            CanSizeLitres = 4m,
            TotalColorantMl = 21.80m,
            TintPercent = 0.55m,
            TotalPrice = 1104.40m,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    public Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DispenseJobResponse?>(new DispenseJobResponse
        {
            Id = 1042,
            ShadeId = 1,
            ShadeCode = "OM-201",
            ShadeName = "Ocean Mist",
            BaseId = 2,
            BaseName = "Medium",
            CanSizeLitres = 4m,
            TotalColorantMl = 21.80m,
            TintPercent = 0.55m,
            TotalPrice = 1104.40m,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    public Task<List<DispenseJobResponse>> GetRecentJobsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetSampleJobs());
    }

    public List<DispenseJobResponse> GetSampleJobs()
    {
        return new List<DispenseJobResponse>
        {
            new() { Id = 1042, ShadeName = "Ocean Mist", ShadeCode = "OM-201", BaseName = "Medium", CanSizeLitres = 4m, TotalColorantMl = 21.80m, TintPercent = 0.55m, TotalPrice = 1104.40m },
            new() { Id = 1041, ShadeName = "Terracotta", ShadeCode = "TR-115", BaseName = "Deep", CanSizeLitres = 10m, TotalColorantMl = 627.50m, TintPercent = 6.28m, TotalPrice = 3451.60m },
            new() { Id = 1040, ShadeName = "Soft White", ShadeCode = "SW-101", BaseName = "Pastel", CanSizeLitres = 4m, TotalColorantMl = 2.00m, TintPercent = 0.05m, TotalPrice = 1001.90m },
            new() { Id = 1039, ShadeName = "Sage Green", ShadeCode = "SG-205", BaseName = "Medium", CanSizeLitres = 1L, TotalColorantMl = 16.50m, TintPercent = 1.65m, TotalPrice = 286.00m }
        };
    }
}
