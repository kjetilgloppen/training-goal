using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingGoal.Api.Auth;

namespace TrainingGoal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GoogleAuthStatus _google;

    public AuthController(GoogleAuthStatus google) => _google = google;

    /// <summary>Starts the Google sign-in flow; Google redirects back to /signin-google, then to "/".</summary>
    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        if (!_google.Configured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Google sign-in is not configured (GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET)." });

        return Challenge(
            new AuthenticationProperties { RedirectUri = "/" },
            GoogleDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { authenticated = false });
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public IActionResult Me() =>
        Ok(new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            name = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
        });
}
