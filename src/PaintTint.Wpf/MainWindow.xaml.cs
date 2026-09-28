using System.Windows;
using System.Windows.Input;
using PaintTint.Wpf.Services;
using PaintTint.Wpf.ViewModels;

namespace PaintTint.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow() : this(new MainViewModel(new TintApiClient("http://localhost:5000")))
    {
    }

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();

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