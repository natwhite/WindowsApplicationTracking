using System.Collections.ObjectModel;
using System.Windows;
using DesktopApplication.di;

namespace DesktopApplication;

public partial class SettingsWindow : Window
{
    // We'll maintain an observable collection 
    // so changes are reflected in the ListBox automatically.
    private ObservableCollection<string> _excludedProcessesCollection
        = new ObservableCollection<string>();

    private StorageService _storageService;

    public SettingsWindow(
        StorageService storageService
    )
    {
        _storageService = storageService;

        InitializeComponent();

        // Copy the static list into an ObservableCollection
        foreach (var process in _storageService.ExcludedProcesses)
        {
            _excludedProcessesCollection.Add(process);
        }

        // Bind the ListBox to that collection
        ExcludedProcessesListBox.ItemsSource = _excludedProcessesCollection;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var processName = NewProcessTextBox.Text?.Trim();

        // If the entered string is empty, do nothing
        // TODO : Consider showing a help message to the user
        // TODO : Function should not use early return without logging the reason
        if (string.IsNullOrEmpty(processName)) return;

        // Update the static logic
        _storageService.AddExcludedProcess(processName);

        // Add to local collection so UI sees it
        if (!_excludedProcessesCollection.Contains(processName))
        {
            _excludedProcessesCollection.Add(processName);
        }

        // Clear the textbox
        NewProcessTextBox.Text = string.Empty;
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        // We can remove multiple selected items
        var selected = ExcludedProcessesListBox.SelectedItems;

        // Do nothing if there are no processes selected for removal
        // TODO : Consider greying out the remove button if no processes are selected
        if (selected.Count <= 0) return;

        // Because selected items is a separate list,
        // let's collect them first, then remove them
        var toRemove = new List<string>();
        foreach (var item in selected)
        {
            // TODO : Ensure the type casting here is safe
            toRemove.Add(item as string);
        }

        foreach (var processName in toRemove)
        {
            if (!_storageService.RemoveExcludedProcess(processName)) continue;

            _excludedProcessesCollection.Remove(processName);
        }
    }
}