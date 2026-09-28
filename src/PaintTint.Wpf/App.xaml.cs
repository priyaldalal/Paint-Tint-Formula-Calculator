using System.Windows;

namespace PaintTint.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--capture-screenshots") || (e.Args != null && e.Args.Contains("--capture-screenshots")))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            ScreenshotGenerator.Generate();
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
