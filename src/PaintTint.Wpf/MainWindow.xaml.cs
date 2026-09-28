using System.Windows;
using System.Windows.Input;
using PaintTint.Wpf.Services;
using PaintTint.Wpf.ViewModels;

namespace PaintTint.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var apiClient = new TintApiClient("http://localhost:5000");
        _viewModel = new MainViewModel(apiClient);
        DataContext = _viewModel;

        Loaded += async (s, e) =>
        {
            await _viewModel.InitializeAsync();
        };
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.SearchShadesCommand.Execute(null);
            e.Handled = true;
        }
    }
}