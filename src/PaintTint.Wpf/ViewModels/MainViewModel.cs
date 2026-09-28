using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaintTint.Core.DTOs;
using PaintTint.Wpf.Interfaces;
using PaintTint.Wpf.Services;

namespace PaintTint.Wpf.ViewModels;

/// <summary>
/// Represents a can volume selection option (e.g. 1L, 4L, 10L, 20L).
/// </summary>
public class CanSizeOption
{
    /// <summary>
    /// Gets or sets the volume in litres.
    /// </summary>
    public decimal Litres { get; set; }

    /// <summary>
    /// Gets or sets the friendly display label (e.g., "4 L").
    /// </summary>
    public string Display { get; set; } = string.Empty;

    /// <summary>
    /// Returns the display string.
    /// </summary>
    public override string ToString() => Display;
}

/// <summary>
/// Primary ViewModel driving the Paint Tint Dispenser Workstation application.
/// Manages shade selection, base selection, volume calculation, validation feedback, and dispensing.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ITintApiClient _apiClient;
    private readonly DispatcherTimer _toastTimer;
    private readonly DispatcherTimer _connectionPollingTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client service for communicating with the PaintTint REST backend.</param>
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

    /// <summary>
    /// Search filter text for looking up paint shades by name or code.
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Collection of filtered shades currently displayed in the selection list.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ShadeSummaryDto> _shades = new();

    /// <summary>
    /// Currently selected shade.
    /// </summary>
    [ObservableProperty]
    private ShadeSummaryDto? _selectedShade;

    /// <summary>
    /// Available paint bases (e.g., Pastel, Medium, Deep).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<BaseDto> _bases = new();

    /// <summary>
    /// Currently selected paint base.
    /// </summary>
    [ObservableProperty]
    private BaseDto? _selectedBase;

    /// <summary>
    /// Available standard container sizes (1L, 4L, 10L, 20L).
    /// </summary>
    public List<CanSizeOption> CanSizes { get; }

    /// <summary>
    /// Currently selected container size.
    /// </summary>
    [ObservableProperty]
    private CanSizeOption _selectedCanSize;

    /// <summary>
    /// Colorant line items calculated for the current formulation.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CalculatedColorantItemDto> _calculatedItems = new();

    /// <summary>
    /// Indicates whether formula colorant items are available and displayed.
    /// </summary>
    [ObservableProperty]
    private bool _hasCalculatedItems;

    /// <summary>
    /// Total volume of colorants required in millilitres.
    /// </summary>
    [ObservableProperty]
    private decimal _totalColorantMl;

    /// <summary>
    /// Actual tint percentage calculated for the current batch.
    /// </summary>
    [ObservableProperty]
    private decimal _tintPercent;

    /// <summary>
    /// Maximum allowed tint percentage for the selected paint base.
    /// </summary>
    [ObservableProperty]
    private decimal _maxTintPercent;

    /// <summary>
    /// Total calculated retail price in INR (Base cost + colorant costs).
    /// </summary>
    [ObservableProperty]
    private decimal _totalPrice;

    /// <summary>
    /// Formatted total price string with currency symbol.
    /// </summary>
    [ObservableProperty]
    private string _totalPriceDisplay = "₹0.00";

    /// <summary>
    /// Text indicator comparing actual tint percentage vs maximum limit.
    /// </summary>
    [ObservableProperty]
    private string _tintProgressText = "0.00% of 0.00%";

    /// <summary>
    /// Progress bar percentage value (0 to 100%).
    /// </summary>
    [ObservableProperty]
    private double _tintProgressValue;

    /// <summary>
    /// Indicates whether the current formulation is valid and ready to dispense.
    /// </summary>
    [ObservableProperty]
    private bool _isValid;

    /// <summary>
    /// Validation error message if the formula exceeds limits.
    /// </summary>
    [ObservableProperty]
    private string? _inlineErrorMessage;

    /// <summary>
    /// Indicates whether an inline validation error alert should be shown.
    /// </summary>
    [ObservableProperty]
    private bool _hasInlineError;

    /// <summary>
    /// Indicates whether an asynchronous operation is in progress.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DispenseCommand))]
    private bool _isBusy;

    /// <summary>
    /// Message displayed in the loading spinner overlay.
    /// </summary>
    [ObservableProperty]
    private string _busyMessage = "Loading...";

    /// <summary>
    /// Indicates whether the WPF client is actively connected to the API backend.
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    /// <summary>
    /// Status bar connection description text.
    /// </summary>
    [ObservableProperty]
    private string _connectionStatusText = "Connecting to API...";

    /// <summary>
    /// Status bar text displaying the most recent completed job number.
    /// </summary>
    [ObservableProperty]
    private string _lastJobText = "Last job: None";

    /// <summary>
    /// Indicates whether the toast notification banner is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool _isToastVisible;

    /// <summary>
    /// Message content displayed in the toast notification.
    /// </summary>
    [ObservableProperty]
    private string _toastMessage = string.Empty;

    /// <summary>
    /// Indicates whether the dispense history slide-over panel is open.
    /// </summary>
    [ObservableProperty]
    private bool _isHistoryOpen;

    /// <summary>
    /// List of recent dispense jobs fetched from the backend.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DispenseJobResponse> _recentJobs = new();

    #endregion

    #region Lifecycle & Initialization

    /// <summary>
    /// Initializes data on startup: connects to API, loads bases, shades, and job history.
    /// </summary>
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
        ConnectionStatusText = connected ? "Connected to API (http://localhost:5000)" : "Offline";
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
            _ = HandleShadeSelectedAsync(value);
        }
        else
        {
            ClearCalculation();
        }
    }

    private async Task HandleShadeSelectedAsync(ShadeSummaryDto shade)
    {
        try
        {
            var detail = await _apiClient.GetShadeByIdAsync(shade.Id);
            if (detail != null && detail.Formulas.Count > 0)
            {
                bool isCurrentBaseValid = SelectedBase != null && detail.Formulas.Any(f => f.BaseId == SelectedBase.Id);
                if (!isCurrentBaseValid)
                {
                    int recommendedBaseId = detail.Formulas[0].BaseId;
                    var matchingBase = Bases.FirstOrDefault(b => b.Id == recommendedBaseId);
                    if (matchingBase != null)
                    {
                        SelectedBase = matchingBase;
                        return; // SelectedBase change will trigger OnSelectedBaseChanged and calculation
                    }
                }
            }
        }
        catch
        {
            // Fallback to direct calculation if shade detail lookup fails
        }

        await CalculateTintAsync();
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

    /// <summary>
    /// Searches available shades using the current search filter term.
    /// </summary>
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

    /// <summary>
    /// Calculates the scaled colorant breakdown and pricing for the currently selected shade, base, and can size.
    /// </summary>
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
            HasCalculatedItems = CalculatedItems.Count > 0;

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
            if (ex.Message.Contains("No formula available", StringComparison.OrdinalIgnoreCase))
            {
                ShowInlineError($"Formula for '{SelectedShade.Name}' ({SelectedShade.Code}) is not available in {SelectedBase.Name} base. Please choose a compatible base.");
            }
            else
            {
                ShowInlineError($"Calculation failed: {ex.Message}");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDispense() => !IsBusy && IsValid && SelectedShade != null && SelectedBase != null;

    /// <summary>
    /// Triggers dispensing for the validated formulation and logs a new dispense job.
    /// </summary>
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

    /// <summary>
    /// Toggles the visibility of the dispense history slide-over panel.
    /// </summary>
    [RelayCommand]
    public async Task ToggleHistoryAsync()
    {
        IsHistoryOpen = !IsHistoryOpen;
        if (IsHistoryOpen)
        {
            await LoadRecentJobsAsync();
        }
    }

    /// <summary>
    /// Closes the dispense history slide-over panel.
    /// </summary>
    [RelayCommand]
    public void CloseHistory()
    {
        IsHistoryOpen = false;
    }

    /// <summary>
    /// Re-checks API connectivity and reloads reference data.
    /// </summary>
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

    /// <summary>
    /// Manually dismisses the toast notification banner.
    /// </summary>
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
        HasCalculatedItems = false;
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
