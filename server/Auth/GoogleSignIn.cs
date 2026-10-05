using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TrainingGoal.Api.Data;
using TrainingGoal.Api.Models;

namespace TrainingGoal.Api.Auth;

/// <summary>Whether Google credentials were configured at startup.</summary>
public record GoogleAuthStatus(bool Configured);

public static class GoogleSignIn
{
    /// <summary>
    /// Runs after Google has authenticated the user, before the app cookie is issued.
    /// Enforces the email allowlist, upserts the local <see cref="User"/> and swaps the
    /// principal for a small one carrying only the app's own claims.
    /// </summary>
    public static async Task OnTicketReceived(TicketReceivedContext ctx)
    {
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var subject = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = ctx.Principal?.FindFirstValue(ClaimTypes.Email);
        var name = ctx.Principal?.FindFirstValue(ClaimTypes.Name);

        var ownerEmail = config["OWNER_EMAIL"];
        if (subject is null || email is null || !IsAllowed(email, ownerEmail, config["ALLOWED_EMAILS"]))
        {
            ctx.HandleResponse();
            ctx.Response.Redirect("/login?error=denied");
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == subject);
        if (user is null)
        {
            user = new User { GoogleSubject = subject, CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
        }
        user.Email = email;
        user.Name = name;
        await db.SaveChangesAsync();

        // Goals from before multi-user support have no owner; hand them to the owner account.
        if (string.Equals(email, ownerEmail, StringComparison.OrdinalIgnoreCase))
        {
            await db.Goals
                .Where(g => g.UserId == null)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.UserId, user.Id));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name ?? user.Email),
        };
        ctx.Principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        ctx.Properties ??= new AuthenticationProperties();
        ctx.Properties.IsPersistent = true;
    }

    public static Task OnRemoteFailure(RemoteFailureContext ctx)
    {
        ctx.HandleResponse();
        ctx.Response.Redirect("/login?error=failed");
        return Task.CompletedTask;
    }

    // Fails closed: with neither OWNER_EMAIL nor ALLOWED_EMAILS set, nobody can sign in.
    private static bool IsAllowed(string email, string? ownerEmail, string? allowedEmails)
    {
        var allowed = (allowedEmails ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Append(ownerEmail ?? "")
            .Where(e => e.Length > 0);

        return allowed.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}
