using System.Diagnostics;
using System.Text;
using DesktopApplication.di;

namespace DesktopApplication;

public record TrackingDetails(
    string WindowTitle,
    string ProcessName
);

public class TrackingService(StorageService storageService)
{
    // For this POC, store excluded processes in a static list in memory
    // private static readonly List<string> _excludedProcesses = new();
    private const int maxWindowChars = 256;

    public TrackingDetails? DetectActiveWindow()
    {
        try
        {
            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Failed to get foreground window handle.");
                return null;
            }

            string windowTitle = GetWindowTitle(hWnd);
            string processName = GetProcessName(hWnd);

            // Check if process is excluded using StorageService
            if (storageService.IsExcludedProcess(processName))
            {
                return null;
            }

            return new TrackingDetails(windowTitle, processName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to detect active window: {ex}");
            return null;
        }
    }

    private string GetWindowTitle(IntPtr hWnd)
    {
        StringBuilder buff = new StringBuilder(maxWindowChars);
        if (NativeMethods.GetWindowText(hWnd, buff, maxWindowChars) > 0)
        {
            return buff.ToString();
        }

        return string.Empty;
    }

    private string GetProcessName(IntPtr hWnd)
    {
        NativeMethods.GetWindowThreadProcessId(hWnd, out int processId);
        try
        {
            Process proc = Process.GetProcessById(processId);
            return proc.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }
}