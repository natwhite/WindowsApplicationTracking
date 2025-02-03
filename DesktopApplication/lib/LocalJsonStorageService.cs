using System.IO;
using System.Text.Json;
using DesktopApplication.di;

namespace DesktopApplication.lib;

public class LocalJsonStorageService : StorageService
{
    // Where we'll store local config/settings
    private string _filePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localConfig.json");

    private HashSet<string> _excludedProcesses { get; set; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public override IReadOnlyCollection<string> ExcludedProcesses => _excludedProcesses;

    public override void AddExcludedProcess(string processName)
    {
        // TODO : Show help text / throw error if the process is already excluded
        if (IsExcludedProcess(processName)) return;

        _excludedProcesses.Add(processName);
    }

    public override bool RemoveExcludedProcess(string processName)
    {
        // We'll do a case-insensitive remove:
        var existing = _excludedProcesses
            .FirstOrDefault(p => p.Equals(processName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            _excludedProcesses.Remove(existing);
            return true;
        }

        return false;
    }

    public override bool IsExcludedProcess(string processName)
    {
        // Compare case-insensitively
        return _excludedProcesses.Any(p => p.Equals(processName, StringComparison.OrdinalIgnoreCase));
    }


    // Call this early in app startup
    public override void Load()
    {
        if (!File.Exists(_filePath)) return; // no file yet

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<LocalConfigData>(json);
            if (data?.ExcludedProcesses != null)
            {
                _excludedProcesses = new HashSet<string>(
                    data.ExcludedProcesses, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex)
        {
            // Log, handle, or ignore. For a POC, just write to console:
            Console.WriteLine($"Error reading local config: {ex}");
        }
    }

    // TODO : Confirm that the processes are being saved properly after refactors
    // Call this before app exit or whenever we want to persist changes
    public override void Save()
    {
        try
        {
            var data = new LocalConfigData
            {
                ExcludedProcesses = _excludedProcesses.ToList()
            };

            var json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing local config: {ex}");
        }
    }

    // Plain old class to represent what we store in JSON
    private class LocalConfigData
    {
        public List<string>? ExcludedProcesses { get; set; }
    }
}