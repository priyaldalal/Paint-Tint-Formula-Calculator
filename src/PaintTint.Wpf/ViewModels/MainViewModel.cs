using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaintTint.Core.DTOs;
using PaintTint.Wpf.Services;

namespace PaintTint.Wpf.ViewModels;

public class CanSizeOption
{
    public decimal Litres { get; set; }
    public string Display { get; set; } = string.Empty;

    public override string ToString() => Display;
}

public partial class MainViewModel : ObservableObject
{
    private readonly ITintApiClient _apiClient;
    private readonly DispatcherTimer _toastTimer;
    private readonly DispatcherTimer _connectionPollingTimer;

    public MainViewModel(ITintApiClient apiClient)
    {
        _apiClient = apiClient;

        CanSizes = new List<CanSizeOption>
        {
            new() { Litres = 1m, Display = "1 L" },
            new() { Litres = 4m, Display = "4 L" },
            new() { Litres = 10m, Display = "10 L" },
            new() { Litres = 20m, Display = "20 L" }
        };
        _selectedCanSize = CanSizes[1]; // Default to 4L

        _toastTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(4)
        };
        _toastTimer.Tick += (s, e) =>
        {
            IsToastVisible = false;
            _toastTimer.Stop();
        };

        _connectionPollingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _connectionPollingTimer.Tick += async (s, e) => await CheckConnectionAsync();
    }

    #region Observable Properties

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ShadeSummaryDto> _shades = new();

    [ObservableProperty]
    private ShadeSummaryDto? _selectedShade;

    [ObservableProperty]
    private ObservableCollection<BaseDto> _bases = new();

    [ObservableProperty]
    private BaseDto? _selectedBase;

    public List<CanSizeOption> CanSizes { get; }

    [ObservableProperty]
    private CanSizeOption _selectedCanSize;

    [ObservableProperty]
    private ObservableCollection<CalculatedColorantItemDto> _calculatedItems = new();

    [ObservableProperty]
    private decimal _totalColorantMl;

    [ObservableProperty]
    private decimal _tintPercent;

    [ObservableProperty]
    private decimal _maxTintPercent;

    [ObservableProperty]
    private decimal _totalPrice;

    [ObservableProperty]
    private string _totalPriceDisplay = "₹0.00";

    [ObservableProperty]
    private string _tintProgressText = "0.00% of 0.00%";

    [ObservableProperty]
    private double _tintProgressValue;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private string? _inlineErrorMessage;

    [ObservableProperty]
    private bool _hasInlineError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DispenseCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyMessage = "Loading...";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatusText = "Connecting to API...";

    [ObservableProperty]
    private string _lastJobText = "Last job: None";

    [ObservableProperty]
    private bool _isToastVisible;

    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private bool _isHistoryOpen;

    [ObservableProperty]
    private ObservableCollection<DispenseJobResponse> _recentJobs = new();

    #endregion

    #region Lifecycle & Initialization

    public async Task InitializeAsync()
    {
        await CheckConnectionAsync();
        await LoadBasesAsync();
        await SearchShadesAsync();
        await LoadLatestJobAsync();
        _connectionPollingTimer.Start();
    }

    private async Task CheckConnectionAsync()
    {
        bool connected = await _apiClient.CheckConnectionAsync();
        IsConnected = connected;
        ConnectionStatusText = connected ? "Connected to API (http://localhost:5000)" : "Disconnected from API (retrying...)";
    }

    private async Task LoadLatestJobAsync()
    {
        try
        {
            var job = await _apiClient.GetLatestJobAsync();
            if (job != null)
            {
                LastJobText = $"Last job: #{job.Id}";
            }
        }
        catch
        {
            // Ignore background error
        }
    }

    private async Task LoadBasesAsync()
    {
        try
        {
            var list = await _apiClient.GetBasesAsync();
            Bases.Clear();
            foreach (var b in list)
            {
                Bases.Add(b);
            }

            // Pick Medium (id 2) or first base
            SelectedBase = Bases.FirstOrDefault(b => b.Name.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                           ?? Bases.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ShowInlineError($"Failed to load paint bases: {ex.Message}");
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnSearchTextChanged(string value)
    {
        // Auto search if cleared or user presses enter
        if (string.IsNullOrWhiteSpace(value))
        {
            _ = SearchShadesAsync();
        }
    }

    partial void OnSelectedShadeChanged(ShadeSummaryDto? value)
    {
        if (value != null)
        {
            _ = OnSelectionChangedAsync();
        }
        else
        {
            ClearCalculation();
        }
    }

    partial void OnSelectedBaseChanged(BaseDto? value)
    {
        if (SelectedShade != null && value != null)
        {
            _ = OnSelectionChangedAsync();
        }
    }

    partial void OnSelectedCanSizeChanged(CanSizeOption value)
    {
        if (SelectedShade != null && SelectedBase != null)
        {
            _ = OnSelectionChangedAsync();
        }
    }

    private async Task OnSelectionChangedAsync()
    {
        if (SelectedShade == null || SelectedBase == null || SelectedCanSize == null)
            return;

        await CalculateTintAsync();
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task SearchShadesAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Searching shades...";
            ClearInlineError();

            var list = await _apiClient.GetShadesAsync(SearchText);
            Shades.Clear();
            foreach (var s in list)
            {
                Shades.Add(s);
            }

            // Auto-select first shade if none selected or current not in list
            if (SelectedShade == null || !Shades.Any(s => s.Id == SelectedShade.Id))
            {
                SelectedShade = Shades.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            ShowInlineError($"Failed to fetch shades: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CalculateTintAsync()
    {
        if (SelectedShade == null || SelectedBase == null || SelectedCanSize == null)
            return;

        try
        {
            IsBusy = true;
            BusyMessage = "Calculating formula...";
            ClearInlineError();

            var req = new CalculateTintRequest
            {
                ShadeId = SelectedShade.Id,
                BaseId = SelectedBase.Id,
                CanSizeLitres = SelectedCanSize.Litres
            };

            var res = await _apiClient.CalculateAsync(req);

            CalculatedItems.Clear();
            foreach (var item in res.Items)
            {
                CalculatedItems.Add(item);
            }

            TotalColorantMl = res.TotalColorantMl;
            TintPercent = res.TintPercent;
            MaxTintPercent = res.MaxTintPercent > 0 ? res.MaxTintPercent : SelectedBase.MaxTintPercent;
            TotalPrice = res.TotalPrice;
            IsValid = res.IsValid;

            // Currency format with Indian Rupee symbol (₹) and thousand separators
            TotalPriceDisplay = string.Format(new CultureInfo("en-IN"), "₹{0:N2}", TotalPrice);

            TintProgressText = $"{TintPercent:F2}% of {MaxTintPercent:G29}%";

            // Visual percentage relative to base max
            if (MaxTintPercent > 0)
            {
                double pct = (double)(TintPercent / MaxTintPercent) * 100.0;
                TintProgressValue = Math.Min(100.0, Math.Max(0.0, pct));
            }
            else
            {
                TintProgressValue = 0;
            }

            if (!res.IsValid && !string.IsNullOrWhiteSpace(res.ValidationError))
            {
                ShowInlineError(res.ValidationError);
            }

            DispenseCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            ClearCalculation();
            ShowInlineError($"Calculation failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDispense() => !IsBusy && IsValid && SelectedShade != null && SelectedBase != null;

    [RelayCommand(CanExecute = nameof(CanDispense))]
    public async Task DispenseAsync()
    {
        if (!CanDispense()) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Dispensing colorants...";
            ClearInlineError();

            var request = new CreateDispenseJobRequest
            {
                ShadeId = SelectedShade!.Id,
                BaseId = SelectedBase!.Id,
                CanSizeLitres = SelectedCanSize.Litres
            };

            var job = await _apiClient.CreateDispenseJobAsync(request);

            LastJobText = $"Last job: #{job.Id}";

            // Show success toast notification
            ShowToast($"Job #{job.Id} dispensed successfully! ({job.TotalColorantMl:F2} ml dispensed)");

            // Refresh recent jobs
            _ = LoadRecentJobsAsync();
        }
        catch (Exception ex)
        {
            ShowInlineError($"Dispense error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ToggleHistoryAsync()
    {
        IsHistoryOpen = !IsHistoryOpen;
        if (IsHistoryOpen)
        {
            await LoadRecentJobsAsync();
        }
    }

    [RelayCommand]
    public void CloseHistory()
    {
        IsHistoryOpen = false;
    }

    [RelayCommand]
    public async Task RefreshConnectionAsync()
    {
        await CheckConnectionAsync();
        if (IsConnected)
        {
            await LoadBasesAsync();
            await SearchShadesAsync();
            await LoadLatestJobAsync();
        }
    }

    [RelayCommand]
    public void DismissToast()
    {
        IsToastVisible = false;
        _toastTimer.Stop();
    }

    #endregion

    #region Helpers

    private async Task LoadRecentJobsAsync()
    {
        try
        {
            var jobs = await _apiClient.GetRecentJobsAsync(15);
            RecentJobs.Clear();
            foreach (var j in jobs)
            {
                RecentJobs.Add(j);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void ShowInlineError(string error)
    {
        InlineErrorMessage = error;
        HasInlineError = true;
    }

    private void ClearInlineError()
    {
        InlineErrorMessage = null;
        HasInlineError = false;
    }

    private void ClearCalculation()
    {
        CalculatedItems.Clear();
        TotalColorantMl = 0m;
        TintPercent = 0m;
        TotalPrice = 0m;
        TotalPriceDisplay = "₹0.00";
        TintProgressText = "0.00% of 0.00%";
        TintProgressValue = 0;
        IsValid = false;
        DispenseCommand.NotifyCanExecuteChanged();
    }

    #endregion
}
