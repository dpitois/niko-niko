using System.Security.Claims;

using AspNet.Security.OAuth.GitHub;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Http; // For HostString
using Microsoft.AspNetCore.Http.Extensions; // For UriHelper
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.Models; // Updated using directive
using NikoNiko.Data; // Updated using directive
using NikoNiko.Services; // Updated using directive

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for handling authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;
    private readonly string _frontendRedirectUrl;

    public AuthController(ApplicationDbContext context, ITokenService tokenService, IConfiguration config, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
        _logger = logger;
        _frontendRedirectUrl = _config["Authentication:FrontendRedirectUrl"] ?? throw new ArgumentNullException("FrontendRedirectUrl is not configured.");
    }

    // /// <summary>
    // /// Initiates the Google login flow.
    // /// </summary>
    // [HttpGet("login-google")]
    // [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger as it's a redirect
    // public IActionResult LoginGoogle()
    // {
    //     var headers = string.Join(", ", Request.Headers.Select(h => $"'{h.Key}': '{h.Value}'"));
    //     _logger.LogInformation("Login-Google Request Headers: [{Headers}]", headers);
    //     return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/signin-google" }, GoogleDefaults.AuthenticationScheme);
    // }

    // /// <summary>
    // /// Google sign-in callback.
    // /// </summary>
    // [HttpGet("signin-google")]
    // [ApiExplorerSettings(IgnoreApi = true)]
    // public async Task<IActionResult> SigninGoogle()
    // {
    //     var (user, token) = await HandleSignIn(GoogleDefaults.AuthenticationScheme);
    //     var redirectUrl = $"{_frontendRedirectUrl}/auth/callback?token={token}";
    //     return Redirect(redirectUrl);
    // }

    // /// <summary>
    // /// Initiates the Microsoft login flow.
    // /// </summary>
    // [HttpGet("login-microsoft")]
    // [ApiExplorerSettings(IgnoreApi = true)]
    // public IActionResult LoginMicrosoft()
    // {
    //     var headers = string.Join(", ", Request.Headers.Select(h => $"'{h.Key}': '{h.Value}'"));
    //     _logger.LogInformation("Login-Microsoft Request Headers: [{Headers}]", headers);
    //     return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/signin-microsoft" }, MicrosoftAccountDefaults.AuthenticationScheme);
    // }

    // /// <summary>
    // /// Microsoft sign-in callback.
    // /// </summary>
    // [HttpGet("signin-microsoft")]
    // [ApiExplorerSettings(IgnoreApi = true)]
    // public async Task<IActionResult> SigninMicrosoft()
    // {
    //     var (user, token) = await HandleSignIn(MicrosoftAccountDefaults.AuthenticationScheme);
    //     var redirectUrl = $"{_frontendRedirectUrl}/auth/callback?token={token}";
    //     return Redirect(redirectUrl);
    // }

    /// <summary>
    /// Initiates the GitHub login flow.
    /// </summary>
    [HttpGet("login-github")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult LoginGitHub(string? invitationToken = null)
    {
        var properties = new AuthenticationProperties { RedirectUri = "/api/auth/signin-github" };

        if (!string.IsNullOrEmpty(invitationToken))
        {
            properties.Items.Add("invitationToken", invitationToken);
        }

        // Force HTTPS for RedirectUri if in Production and X-Forwarded-Proto is HTTPS
        if (_config.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto) && forwardedProto == "https")
            {
                properties.RedirectUri = UriHelper.BuildAbsolute(
                    "https",
                    new HostString(HttpContext.Request.Host.Value),
                    PathString.FromUriComponent(properties.RedirectUri)
                ).ToString();
            }
            // If the application is directly exposed via HTTPS (no proxy or proxy not setting X-Forwarded-Proto)
            else if (HttpContext.Request.IsHttps)
            {
                properties.RedirectUri = UriHelper.BuildAbsolute(
                    "https",
                    new HostString(HttpContext.Request.Host.Value),
                    PathString.FromUriComponent(properties.RedirectUri)
                ).ToString();
            }
        }

        var headers = string.Join(", ", Request.Headers.Select(h => $"'{h.Key}': '{h.Value}'"));
        _logger.LogInformation("Login-GitHub Request Headers: [{Headers}]", headers);
        return Challenge(properties, GitHubAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// GitHub sign-in callback.
    /// </summary>
    [HttpGet("signin-github")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SigninGitHub()
    {
        var headers = string.Join(", ", Request.Headers.Select(h => $"'{h.Key}': '{h.Value}'"));
        _logger.LogInformation("Signin-GitHub Request Headers: [{Headers}]", headers);

        var authenticateResult = await HttpContext.AuthenticateAsync(GitHubAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            _logger.LogError(authenticateResult.Failure, "GitHub authentication failed during callback.");
            throw new Exception($"Error authenticating with GitHub: {authenticateResult.Failure?.Message}");
        }

        string? invitationToken = null;
        if (authenticateResult.Properties != null && authenticateResult.Properties.Items.TryGetValue("invitationToken", out var tokenValue))
        {
            invitationToken = tokenValue;
        }

        var (user, token) = await HandleSignIn(GitHubAuthenticationDefaults.AuthenticationScheme, invitationToken);
        var redirectUrl = $"{_frontendRedirectUrl}/auth/callback?token={token}";
        return Redirect(redirectUrl);
    }

    private async Task<(User, string)> HandleSignIn(string provider, string? invitationToken = null)
    {
        var result = await HttpContext.AuthenticateAsync(provider);
        if (!result.Succeeded)
        {
            _logger.LogError(result.Failure, "Authentication failed for provider {Provider}.", provider);
            throw new Exception($"Error authenticating with {provider}: {result.Failure?.Message}");
        }

        var claims = result.Principal.Claims;
        var oauthId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        // GitHub-specific claims
        var avatar = claims.FirstOrDefault(c => c.Type == "urn:github:avatar_url")?.Value;
        var githubLogin = claims.FirstOrDefault(c => c.Type == "urn:github:login")?.Value;


        if (oauthId == null || name == null)
        {
            throw new Exception("Could not retrieve required user information from provider.");
        }

        // For GitHub, the public email might be null. We'll use a placeholder if needed.
        email ??= $"{githubLogin}@users.noreply.github.com";

        var user = await _context.Users.FirstOrDefaultAsync(u => u.OAuthId == oauthId && u.Email == email);

        var superAdminEmails = GetSuperAdminEmails();

        if (user == null)
        {
            user = new User
            {
                OAuthId = oauthId,
                Email = email,
                Name = name,
                AvatarUrl = avatar
            };

            if (superAdminEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase))
            {
                user.IsSuperAdmin = true;
                _logger.LogInformation("Promoted new user {Email} to super admin.", user.Email);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Sync IsSuperAdmin status for existing users
            var shouldBeSuperAdmin = superAdminEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase);

            if (user.IsSuperAdmin != shouldBeSuperAdmin)
            {
                user.IsSuperAdmin = shouldBeSuperAdmin;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated super admin status for existing user {Email} to {IsSuperAdmin}.", user.Email, user.IsSuperAdmin);
            }
        }

        var token = _tokenService.CreateToken(user);

        // If an invitation token was provided, attempt to accept the invitation
        if (!string.IsNullOrEmpty(invitationToken))
        {
            try
            {
                var teamInvitationService = HttpContext.RequestServices.GetRequiredService<ITeamInvitationService>();
                await teamInvitationService.AcceptTeamInvitationAsync(invitationToken, user.Id);
                _logger.LogInformation("User {UserId} successfully accepted invitation {InvitationToken}.", user.Id, invitationToken);
            }
            catch (Exception ex)
            {
                // Log the error but don't prevent login, as the user is already authenticated.
                _logger.LogError(ex, "Failed to accept invitation {InvitationToken} for user {UserId} during sign-in.", invitationToken, user.Id);
            }
        }
        return (user, token);
    }

    private List<string> GetSuperAdminEmails()
    {
        var superAdminEmailsConfig = _config["SUPER_ADMINS"];
        return !string.IsNullOrWhiteSpace(superAdminEmailsConfig)
            ? superAdminEmailsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : new List<string>();
    }
}