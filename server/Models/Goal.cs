using System.ComponentModel.DataAnnotations;

namespace TrainingGoal.Api.Models;

public class Goal
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    /// <summary>How many times the activity should be done over the period (e.g. 100).</summary>
    public int TargetCount { get; set; }

    /// <summary>Inclusive start of the goal period (date only).</summary>
    public DateOnly PeriodStart { get; set; }

    /// <summary>Inclusive end of the goal period (date only).</summary>
    public DateOnly PeriodEnd { get; set; }

    /// <summary>Optional display label for one occurrence, e.g. "hikes".</summary>
    [MaxLength(50)]
    public string? Unit { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<LogEntry> Logs { get; set; } = new();
}
