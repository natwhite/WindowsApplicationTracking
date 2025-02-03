namespace BlazorApp1.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TimeEntriesController : ControllerBase
{
    private readonly TimeTrackingDbContext _db;

    public TimeEntriesController(TimeTrackingDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _db.TimeEntries.ToListAsync();
        return Ok(entries);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var entry = await _db.TimeEntries.FindAsync(id);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TimeEntry newEntry)
    {
        newEntry.Id = Guid.NewGuid();
        newEntry.CreatedAt = DateTime.UtcNow;
        newEntry.UpdatedAt = DateTime.UtcNow;

        _db.TimeEntries.Add(newEntry);
        
        Console.WriteLine($"Adding new entry: {newEntry}");
        
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = newEntry.Id }, newEntry);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TimeEntry updated)
    {
        var entry = await _db.TimeEntries.FindAsync(id);
        if (entry == null) return NotFound();

        entry.UserId = updated.UserId;
        entry.StartTime = updated.StartTime;
        entry.EndTime = updated.EndTime;
        entry.WindowTitle = updated.WindowTitle;
        entry.ProcessName = updated.ProcessName;
        entry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = await _db.TimeEntries.FindAsync(id);
        if (entry == null) return NotFound();

        _db.TimeEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}