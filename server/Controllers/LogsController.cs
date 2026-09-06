using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var log = await _db.Logs.FindAsync(id);
        if (log is null) return NotFound();

        _db.Logs.Remove(log);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
