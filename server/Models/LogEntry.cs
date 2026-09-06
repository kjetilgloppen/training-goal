using System.ComponentModel.DataAnnotations;

namespace TrainingGoal.Api.Models;

public class LogEntry
{
    public int Id { get; set; }

    public int GoalId { get; set; }
    public Goal? Goal { get; set; }

    /// <summary>The date the activity was performed.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Optional: how long the activity took today, stored in seconds (entered as h/m/s).</summary>
    public int? DurationSeconds { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
}
