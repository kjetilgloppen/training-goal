using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingGoal.Api.Auth;
using TrainingGoal.Api.Data;
using TrainingGoal.Api.Models;

namespace TrainingGoal.Api.Controllers;

[ApiController]
[Route("api/goals")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GoalsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<GoalDto>> GetGoals()
    {
        var userId = User.GetUserId();
        var goals = await _db.Goals
            .Where(g => g.UserId == userId)
            .Include(g => g.Logs.OrderBy(l => l.Date))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return goals.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GoalDto>> GetGoal(int id)
    {
        var userId = User.GetUserId();
        var goal = await _db.Goals
            .Include(g => g.Logs.OrderBy(l => l.Date))
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        return goal is null ? NotFound() : ToDto(goal);
    }

    [HttpPost]
    public async Task<ActionResult<GoalDto>> CreateGoal([FromBody] CreateGoalRequest req)
    {
        if (req.PeriodEnd < req.PeriodStart)
            return BadRequest(new { message = "PeriodEnd must be on or after PeriodStart." });

        var goal = new Goal
        {
            Name = req.Name.Trim(),
            TargetCount = req.TargetCount,
            PeriodStart = req.PeriodStart,
            PeriodEnd = req.PeriodEnd,
            Unit = string.IsNullOrWhiteSpace(req.Unit) ? null : req.Unit.Trim(),
            CreatedAt = DateTime.UtcNow,
            UserId = User.GetUserId(),
        };

        _db.Goals.Add(goal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, ToDto(goal));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var userId = User.GetUserId();
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null) return NotFound();

        _db.Goals.Remove(goal);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Logs nested under a goal ----

    [HttpPost("{goalId:int}/logs")]
    public async Task<ActionResult<LogDto>> AddLog(int goalId, [FromBody] CreateLogRequest req)
    {
        var userId = User.GetUserId();
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal is null) return NotFound(new { message = "Goal not found." });

        var log = new LogEntry
        {
            GoalId = goalId,
            Date = req.Date,
            DurationSeconds = req.DurationSeconds,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();

        return Ok(new LogDto(log.Id, log.Date, log.DurationSeconds, log.Note));
    }

    private static GoalDto ToDto(Goal g) => new(
        g.Id,
        g.Name,
        g.TargetCount,
        g.PeriodStart,
        g.PeriodEnd,
        g.Unit,
        g.Logs.Select(l => new LogDto(l.Id, l.Date, l.DurationSeconds, l.Note)).ToList());
}
