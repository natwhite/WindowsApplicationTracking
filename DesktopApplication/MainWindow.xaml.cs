using System.Collections.ObjectModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace DesktopApplication;

public partial class MainWindow : Window
{
    private DataService _dataService;

    // This is bound to the ItemsControl in XAML
    public ObservableCollection<TimeEntry> TimeEntries { get; set; }
        = new ObservableCollection<TimeEntry>();

    public MainWindow(DataService dataService)
    {
        _dataService = dataService;

        InitializeComponent();

        // Set the data context so XAML can access `TimeEntries`
        this.DataContext = this;
    }
    
    protected override async void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        // For demonstration: fetch today's entries
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        try
        {
            var entries = await _dataService.GetTimeEntriesAsync(today, tomorrow);

            // Sort them by startTime so they appear top to bottom
            entries.Sort((a, b) => a.startTime.CompareTo(b.startTime));

            foreach (var entry in entries)
            {
                TimeEntries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            WinForms.MessageBox.Show($"Error retrieving time entries: {ex}");
        }
    }
}