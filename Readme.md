# Time Tracker WPF Application

A **time-tracking** and **active window detection** application in **C# (.NET)**. This project demonstrates how to:

- Detect the currently active window and its associated process name.
- Track changes (e.g., when the user switches between windows).
- Exclude certain processes from tracking.
- Store these exclusions in a local JSON file for persistence.
- Send data to a REST API running on ASP.NET (or another backend).
- Display a timeline of time entries (start/end times for each window) in a **WPF** application.
- Run as a **tray application** that remains active even when no main window is open.

## Features

1. **Active Window Detection**
    - Uses Win32 API calls (`GetForegroundWindow`, `GetWindowText`, `GetWindowThreadProcessId`) to detect the current foreground window.
    - Captures **window title** and **process name**.

2. **Tracking Logic**
    - Only sends a “time entry” to the backend when there is a **change** in the focused window (to avoid spamming).
    - Timestamp-based: stores the start time of a window focus and the end time (when switching away).

3. **Exclusion List**
    - Users can **exclude** certain processes from tracking (e.g., `explorer`, `cmd`, etc.).
    - Stored locally in a JSON file, loaded/saved at startup/shutdown.

4. **Tray Icon**
    - Runs in the **system tray** instead of a visible window.
    - The application does **not** shut down when the main window is closed (via `OnExplicitShutdown` or manual management).
    - Context menu allows easy opening of a **Settings** window or a **Timeline** window.

5. **WPF Timeline Window**
    - Displays previously tracked time entries (fetched from the backend) in a chronological list (or vertical layout).
    - Provides a visual overview of which applications were used, and at which times, during the day.

6. **REST API Integration**
    - Uses `HttpClient` to **POST** new time entries to an external API endpoint (e.g., `/api/timeentries`).
    - Uses `HttpClient` to **GET** time entries from the backend to display in the timeline.

## Project Structure

```
DesktopApplication/
├─ App.xaml               # Main WPF application definition (no StartupUri, or set to OnExplicitShutdown)
├─ App.xaml.cs           # Application startup; creates tray icon, configures DI/services, starts tracking
├─ MainWindow.xaml       # Timeline display window
├─ MainWindow.xaml.cs    # MainWindow code-behind logic
├─ SettingsWindow.xaml   # UI for excluded process management
├─ SettingsWindow.xaml.cs # Code-behind for managing the exclusion list
├─ di/
│  ├─ StorageService.cs  # Abstract class defining storage contract
├─ lib/
│  ├─ LocalJsonStorageService.cs # Implementation of StorageService that saves to a local JSON file
├─ TrackingMethods.cs    # Detects currently active window (process & title)
├─ DataService.cs        # Sends and retrieves time entries to/from the backend API
├─ TrackingService.cs    # Coordinates the tracking loop and logic (optional, or in App.xaml.cs)
└─ ... other files ...
```

### Key Files

- **`TrackingMethods.cs`**
    - **Static** (or instance-based) class that queries the active window using Win32 API (P/Invoke).
    - Checks against the stored exclusion list to skip certain processes.

- **`LocalJsonStorageService.cs`**
    - Inherits from the abstract `StorageService`.
    - Loads, saves, and manages excluded processes in a local JSON file (`localConfig.json`).

- **`DataService.cs`**
    - Encapsulates **HTTP client** logic, including:
        - `SendDataToServer` to submit a time entry.
        - `GetTimeEntriesAsync` to retrieve time entries for display.

- **`App.xaml.cs`**
    - The application’s **entry point**.
    - Creates the **tray icon** (`NotifyIcon` from `System.Windows.Forms`), context menu, and background tracking loop.
    - Instantiates `DataService`, `LocalJsonStorageService`, etc.
    - Manages **lifetime** so the app continues running until the user explicitly exits.

- **`MainWindow.xaml` & `.cs`**
    - The **timeline** window that shows a vertical list of previously tracked entries fetched from the backend.

- **`SettingsWindow.xaml` & `.cs`**
    - A UI to manage the local list of excluded processes (add/remove).
    - Calls methods on the `StorageService` to persist changes.

## How It Works

1. **Startup**
    - `App.xaml.cs` runs `OnStartup`.
    - It loads local settings (excluded processes) from `LocalJsonStorageService`.
    - Creates a **tray icon** with a context menu.
    - Starts the **tracking loop** (`StartTracking()`), which periodically checks the active window:
        - If it changes from the previously tracked window, it sends a **time entry** to the server using `DataService`.
        - Ignores any process names that are in the exclusion list.

2. **Tray Menu**
    - “Settings”: opens the **`SettingsWindow`**, allowing add/remove of excluded processes.
    - “Timeline”: opens the **`MainWindow`**, which fetches existing tracked entries from the server and displays them.
    - “Exit”: calls `Application.Current.Shutdown()` to exit the app cleanly.

3. **MainWindow (Timeline)**
    - On load (`OnInitialized`), calls `DataService.GetTimeEntriesAsync(...)` to retrieve the day’s entries.
    - Displays them in a vertical layout (or list) from earliest to latest.

4. **Shut Down**
    - User selects “Exit” from the tray, or calls `Application.Current.Shutdown()`.
    - `OnExit` saves the local config (exclusions) again.
    - The tray icon is disposed.

## Requirements

- **.NET 6+** (or .NET 7/8/9)
- **WPF** support (Windows-only)
- A **REST API** endpoint for storing time entries (optional for the POC, can be disabled if not testing server communication).

## Installation & Run

1. **Clone** this repo:
   ```bash
   git clone https://github.com/<your-username>/TimeTrackerWpf.git
   ```
2. **Open** the solution in your favorite IDE (JetBrains Rider, Visual Studio, etc.).
3. **Restore** packages:
   ```bash
   dotnet restore
   ```
4. (Optional) **Configure** the base API URI in `App.xaml.cs` or wherever `_dataService` is instantiated.
5. **Run** the project in debug mode or release mode. You’ll see a tray icon appear.
6. **Click** the tray icon’s menu to open the timeline or settings.

## Usage

- **Settings Window**
    - Allows you to add or remove process names from the exclusion list.
    - Example: type `rider64` to exclude JetBrains Rider. Then click “Add.”
- **Timeline Window**
    - Shows a chronological list of entries: start time, end time, window title, process name.
    - The data is fetched from your configured backend.
- **Exit**
    - Right-click (or left-click) the tray icon, choose “Exit.” The app will then save your local config and terminate.

## Roadmap / To-Do

- [ ] Add **authentication** to the REST calls.
- [ ] Provide date-range selection in the timeline.
- [ ] Improve the timeline UI (graphical blocks vs. text).
- [ ] Add logging (e.g., `Serilog`) for debug info vs. console writes.
- [ ] Potentially wrap everything in **Microsoft.Extensions.DependencyInjection** or another DI framework for more scalable architecture.

## Contributing

1. **Fork** the repo.
2. **Create** a feature branch: `git checkout -b feature/awesome-update`.
3. **Commit** your changes: `git commit -m "Add new awesome feature"`.
4. **Push** the branch: `git push origin feature/awesome-update`.
5. **Create** a pull request.

We welcome contributions, suggestions, and bug reports. Feel free to submit issues or PRs!

## License

*(You can choose a license that suits you—MIT, Apache, etc. If not specified, add one. Example:)*

Licensed under the [MIT License](LICENSE).

---

**Thank you** for checking out this WPF Time Tracker project! If you have questions or find issues, please open an [issue](https://github.com/your-username/TimeTrackerWpf/issues) or submit a pull request. Happy coding!