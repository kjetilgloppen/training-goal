using System.Security.Claims;

namespace TrainingGoal.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The app's own user id, stored as the NameIdentifier claim at sign-in.</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Signed-in user has no id claim."));
}
