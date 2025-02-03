namespace DesktopApplication.di;

public abstract class StorageService
{
    public abstract IReadOnlyCollection<string> ExcludedProcesses { get; }

    /// <summary>
    /// Add a process name to the exclusion list, if not already present.
    /// </summary>
    public abstract void AddExcludedProcess(string processName);

    /// <summary>
    /// Remove a process name from the exclusion list.
    /// </summary>
    public abstract bool RemoveExcludedProcess(string processName);

    public abstract bool IsExcludedProcess(string processName);

    // Call this early in app startup
    public abstract void Load();

    // Call this before app exit or whenever we want to persist changes
    public abstract void Save();
}