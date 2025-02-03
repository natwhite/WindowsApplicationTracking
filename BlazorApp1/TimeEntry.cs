namespace BlazorApp1;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TimeEntries")]
public class TimeEntry
{
    [Key] public Guid Id { get; set; }

    public String UserId { get; set; }
    // Or a Guid, if we prefer to store full user data.

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [MaxLength(500)] public string WindowTitle { get; set; } = string.Empty;

    [MaxLength(100)] public string ProcessName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}