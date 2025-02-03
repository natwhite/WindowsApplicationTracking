using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DesktopApplication;

// existing record for sending to server
public record TimeEntry(
    string userId,
    DateTime startTime,
    DateTime endTime,
    string windowTitle,
    string processName
)
{
    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class DataService(string baseUri)
{
    private readonly string _baseUri = baseUri.TrimEnd('/');
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task SendDataToServer(
        TrackingDetails trackingDetails,
        DateTime startTime,
        DateTime endTime
    )
    {
        TimeEntry dateEntry = new TimeEntry(
            "default",
            startTime,
            endTime,
            trackingDetails.WindowTitle,
            trackingDetails.ProcessName
        );

        var json = dateEntry.Serialize();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // e.g. POST /api/timeentries
        var response = await _httpClient.PostAsync($"{_baseUri}/api/timeentries", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TimeEntry>> GetTimeEntriesAsync(
        DateTime? start = null,
        DateTime? end = null
    )
    {
        // Build query param string if you want date-range filtering
        // e.g. ?start=2025-02-03T00:00:00Z&end=2025-02-03T23:59:59Z
        var queryParams = new List<string>();
        if (start.HasValue) queryParams.Add($"start={start.Value:O}");
        if (end.HasValue) queryParams.Add($"end={end.Value:O}");

        var queryString = queryParams.Any()
            ? "?" + string.Join("&", queryParams)
            : string.Empty;

        var response = await _httpClient.GetAsync($"{_baseUri}/api/timeentries{queryString}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var entries = JsonSerializer.Deserialize<List<TimeEntry>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return entries ?? new List<TimeEntry>();
    }
}