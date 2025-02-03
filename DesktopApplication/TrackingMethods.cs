using System.Diagnostics;
using System.Text;

namespace DesktopApplication;

public record TrackingDetails(
    string WindowTitle,
    string ProcessName
);

public static class TrackingMethods
{
    public static TrackingDetails? DetectActiveWindow()
    {
        try
        {
            // Example method to get the foreground window/process
            IntPtr hWnd = NativeMethods.GetForegroundWindow();

            // Could mean no window is in the foreground, or we don't have permission to see it.s
            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to get foreground window handle");
                return null;
            }

            string windowTitle = GetWindowTitle(hWnd);
            string processName = GetProcessName(hWnd);
            // TODO: Send this data via HTTP to the backend

            // return  $"Active Window: \"{windowTitle}\", Process: {processName}";
            return new TrackingDetails(windowTitle, processName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to detect active window: {ex}");
            return null;
        }
    }

    /// <summary>
    /// Returns the window title for the given window handle.
    /// </summary>
    private static string GetWindowTitle(IntPtr hWnd)
    {
        const int nChars = 256; // Arbitrary max-size for window title text
        StringBuilder buff = new StringBuilder(nChars);
        if (NativeMethods.GetWindowText(hWnd, buff, nChars) > 0)
        {
            return buff.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns the process name associated with the given window handle.
    /// </summary>
    private static string GetProcessName(IntPtr hWnd)
    {
        NativeMethods.GetWindowThreadProcessId(hWnd, out int processId);
        try
        {
            Process proc = Process.GetProcessById(processId);
            return proc.ProcessName;
        }
        catch
        {
            // If for some reason the process doesn't exist or can't be accessed
            return "Unknown";
        }
    }
}