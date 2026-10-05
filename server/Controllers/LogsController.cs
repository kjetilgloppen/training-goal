using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingGoal.Api.Auth;
using TrainingGoal.Api.Data;

namespace TrainingGoal.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LogsController(AppDbContext db) => _db = db;

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        var userId = User.GetUserId();
        var log = await _db.Logs.FirstOrDefaultAsync(l => l.Id == id && l.Goal!.UserId == userId);
        if (log is null) return NotFound();

        _db.Logs.Remove(log);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
