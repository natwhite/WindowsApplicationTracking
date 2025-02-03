using System.Windows;
using System.Windows.Media;
using DesktopApplication.di;
using DesktopApplication.lib; // for Application, StartupEventArgs
using WinForms = System.Windows.Forms; // alias for System.Windows.Forms
using Drawing = System.Drawing; // alias for System.Drawing

namespace DesktopApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    // System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
    private WinForms.NotifyIcon? _trayIcon;
    private WinForms.ContextMenuStrip? _trayMenu;
    private DataService _dataService;
    private StorageService _storageService;
    private TrackingService _trackingService;
    private const string baseUri = "http://localhost:5274/";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialized the DataService class
        _dataService = new DataService(baseUri);

        // Initialize Local JSON storage as Storage Service
        _storageService = new LocalJsonStorageService();
        _storageService.Load();
        
        _trackingService = new TrackingService(_storageService);

        // Create the WinForms context menu for the tray
        _trayMenu = new WinForms.ContextMenuStrip();
        // TODO : Consider collecting strings into separate class to handle different languages
        _trayMenu.Items.Add("TimeLine", null, OnTimelineClick);
        _trayMenu.Items.Add("Settings", null, OnSettingsClick);
        _trayMenu.Items.Add("Exit", null, OnExitClick);

        // Create the tray icon
        _trayIcon = new WinForms.NotifyIcon
        {
            Text = "My Time Tracker (WPF)",
            Icon = Drawing.SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };

        // Start background tracking
        StartTracking();
    }

    private async void StartTracking(long pollPeriodSeconds = 5)
    {
        try
        {
            TrackingDetails? previousWindow = null;
            DateTime previousTimestamp = DateTime.UtcNow;

            while (true)
            {
                // Moving the delay to the start to allow for early termination with a guaranteed delay
                await Task.Delay(TimeSpan.FromSeconds(pollPeriodSeconds));

                TrackingDetails? windowDetails = _trackingService.DetectActiveWindow();
                DateTime timestamp = DateTime.UtcNow;

                // If we were unable to retrieve window data, just move on.
                if (windowDetails == null) continue;

                // If this is the first time getting window data, set the previous window and continue
                if (previousWindow == null)
                {
                    previousWindow = windowDetails;
                    previousTimestamp = timestamp;
                    Console.WriteLine($"No previous window found, setting up tracking for {windowDetails.ProcessName}");
                    continue; // We want to track intervals, so we'll wait until we see a change in state
                }

                // Check to see if the current window is the same as the previous window
                if (
                    windowDetails.WindowTitle == previousWindow.WindowTitle
                    && windowDetails.ProcessName == previousWindow.ProcessName
                )
                {
                    // TODO : Set up logger and log the following line as Verbose
                    // Console.WriteLine("No change in tracking window, skipping.");
                    continue; // If the window details are the same, continue waiting
                }

                // Submit tracking chunk to backend server
                Console.WriteLine($"Submitting tracking for process: {previousWindow.ProcessName}");
                await _dataService.SendDataToServer(
                    previousWindow,
                    previousTimestamp,
                    timestamp
                );

                // Update the previous window details and timestamp
                Console.WriteLine($"Now tracking focused process: {windowDetails.ProcessName}");
                previousWindow = windowDetails;
                previousTimestamp = timestamp;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected exception while running main tracking function: {e}");
            Console.WriteLine("Terminating...");
            Shutdown();
        }
    }

    private void OnTimelineClick(object? sender, EventArgs e)
    {
        // Instantiate MainWindow ourselves so we can pass dependencies
        // We had to remove MainWindow from the xaml to prevent auto 0 param init
        var mainWindow = new MainWindow(_dataService);
        mainWindow.Show();
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        var settingsWindow = new SettingsWindow(_storageService);
        settingsWindow.ShowDialog();

        // TODO : Implement WPF settings window
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        _trayIcon!.Visible = false;
        _trayIcon.Dispose();

        // Properly shut down the WPF application
        // TODO : Consider trying to publish the most recent event before existing
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Clean up
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        base.OnExit(e);
    }
}