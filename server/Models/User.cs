using System.ComponentModel.DataAnnotations;

namespace TrainingGoal.Api.Models;

public class User
{
    public int Id { get; set; }

    /// <summary>Google's stable account id (the OIDC "sub" claim).</summary>
    [Required, MaxLength(100)]
    public string GoogleSubject { get; set; } = "";

    [Required, MaxLength(320)]
    public string Email { get; set; } = "";

    [MaxLength(200)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Goal> Goals { get; set; } = new();
}
