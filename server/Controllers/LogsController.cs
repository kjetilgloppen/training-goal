using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingGoal.Api.Auth;
using TrainingGoal.Api.Data;

namespace TrainingGoal.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogsController(AppDbContext db) : ControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LogDto>> UpdateLog(int id, [FromBody] CreateLogRequest req)
    {
        var userId = User.GetUserId();
        var log = await db.Logs.FirstOrDefaultAsync(l => l.Id == id && l.Goal!.UserId == userId);
        if (log is null) return NotFound();

        log.Date = req.Date;
        log.DurationSeconds = req.DurationSeconds;
        log.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();

        await db.SaveChangesAsync();
        return new LogDto(log.Id, log.Date, log.DurationSeconds, log.Note);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        var userId = User.GetUserId();
        var log = await db.Logs.FirstOrDefaultAsync(l => l.Id == id && l.Goal!.UserId == userId);
        if (log is null) return NotFound();

        db.Logs.Remove(log);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
