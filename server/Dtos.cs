using System.ComponentModel.DataAnnotations;

namespace TrainingGoal.Api;

// ---- Auth ----
public record LoginRequest([Required] string Passcode);

// ---- Goals ----
public record CreateGoalRequest(
    [Required, MaxLength(200)] string Name,
    [Range(1, 100000)] int TargetCount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string? Unit);

public record LogDto(int Id, DateOnly Date, int? DurationSeconds, string? Note);

public record GoalDto(
    int Id,
    string Name,
    int TargetCount,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string? Unit,
    List<LogDto> Logs);

// ---- Logs ----
public record CreateLogRequest(
    DateOnly Date,
    [Range(0, 1000000)] int? DurationSeconds,
    [MaxLength(500)] string? Note);
