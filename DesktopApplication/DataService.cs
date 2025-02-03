using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DesktopApplication;

record TimeEntry(
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
};

public class DataService
{
    private string _uri;
    private static readonly HttpClient _httpClient = new HttpClient();

    public DataService(string uri)
    {
        _uri = uri;
    }

    // TODO : Consider refactoring internal code to use DateTime instead of long for dates
    public async Task SendDataToServer(
        TrackingDetails trackingDetails,
        // long startTime,
        // long endTime
        DateTime startTime,
        DateTime endTime
    )
    {
        string TimestampToString(long ts) =>
            DateTimeOffset.FromUnixTimeSeconds(ts)
                .DateTime.ToString("HH:mm:ss");

        // string startTimeString = TimestampToString(startTime);
        // string endTimeString = TimestampToString(endTime);

        // Console.WriteLine($"Interval {startTimeString} - {endTimeString}:");
        Console.WriteLine($"Interval {startTime} - {endTime}:");
        Console.WriteLine($"\tActive Window: \"{trackingDetails.WindowTitle}\"");
        Console.WriteLine($"\tProcess: {trackingDetails.ProcessName}");

        TimeEntry dateEntry = new TimeEntry(
            "default",
            startTime,
            endTime,
            // DateTimeOffset.FromUnixTimeSeconds(startTime).DateTime,
            // DateTimeOffset.FromUnixTimeSeconds(endTime).DateTime,
            trackingDetails.WindowTitle,
            trackingDetails.ProcessName
        );

        var json = dateEntry.Serialize();

        Console.WriteLine($"Sending data to server: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // TODO : Replace this with an actual endpoint
        // TODO : Refactor the URI to a environment variable. 
        var response = await _httpClient.PostAsync(_uri, content);
        response.EnsureSuccessStatusCode();
    }
}