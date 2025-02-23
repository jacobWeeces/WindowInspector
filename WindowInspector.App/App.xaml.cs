using System.Configuration;
using System.Data;
using System.Windows;
using WindowInspector.App.Services;

namespace WindowInspector.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logger
        var logger = Logger.Instance;
        logger.Info("Application starting...");

        // Handle unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            logger.Error(args.Exception);
            MessageBox.Show(
                $"An unexpected error occurred. Check the logs for details.\n\nError: {args.Exception.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            if (ex != null)
            {
                logger.Error(ex);
            }
            else
            {
                logger.Error($"Unknown error occurred: {args.ExceptionObject}");
            }
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            logger.Error(args.Exception);
            args.SetObserved();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Instance.Info("Application shutting down...");
        Logger.Instance.Dispose();
        base.OnExit(e);
    }
}

