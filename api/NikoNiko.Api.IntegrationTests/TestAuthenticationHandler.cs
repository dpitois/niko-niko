using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NikoNiko.Api.IntegrationTests;

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract invitation token from request query if present, to be passed back in properties
        // This mimics the behavior of the real OAuth handler preserving state/properties
        string? invitationToken = null;
        if (Request.Query.TryGetValue("invitationToken", out var tokenValues))
        {
            invitationToken = tokenValues.ToString();
        }
        // Also check if it was stored in AuthenticationProperties during Challenge (not easily accessible here without a context flow, 
        // but typically in a real flow, the state parameter handles this. 
        // For this test handler, we might assume the test sets up the context or we just return a fixed user.)
        
        // However, the AuthController looks for "invitationToken" in authenticateResult.Properties.Items
        // We need to ensure that if the original Challenge had properties, they are preserved.
        // But HandleAuthenticateAsync creates the result.
        
        // Let's create a standard user. The test "AcceptInvitation..." expects a NEW user.
        // We should probably make the user configurable or dynamic, but for now let's fix the "No handler" error.
        // The Controller's SigninGitHub calls AuthenticateAsync("GitHub").
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "github|newtestuser"), // Distinct from seeded users
            new Claim(ClaimTypes.Name, "New Test User"),
            new Claim(ClaimTypes.Email, "newtestuser@example.com"),
            new Claim("urn:github:login", "newtestuser"),
            new Claim("urn:github:avatar_url", "https://example.com/avatar.png")
        };
        
        var identity = new ClaimsIdentity(claims, "GitHub");
        var principal = new ClaimsPrincipal(identity);
        
        var properties = new AuthenticationProperties();
        // Crucial: The controller retrieves the invitation token from properties.Items["invitationToken"]
        // In a real OAuth flow, this is round-tripped via the 'state' parameter.
        // In our fake, we need to ensure this property is present if the test expects it.
        // Since we can't easily see the Properties passed to Challenge() here (it's a stateless HTTP request in the test context usually),
        // we might need to rely on the test passing it back or just hardcode it if the test is specific.
        
        // Wait, the test calls client.GetAsync("/api/auth/signin-github?invitationToken=..."); ??
        // No, the test flow is:
        // 1. Client calls /api/auth/login-github?invitationToken=XYZ
        // 2. Controller returns Challenge(properties, "GitHub")
        // 3. TestHost middleware intercepts Challenge? Or does it return 401/302?
        // 4. In integration tests with WebApplicationFactory, Challenge usually returns 401 or redirects to login page.
        // 5. To verify the callback, the TEST manually calls /api/auth/signin-github.
        // 6. The controller calls HttpContext.AuthenticateAsync("GitHub").
        
        // PROBLEM: The `AuthenticateAsync` call in the controller expects to find the `AuthenticationProperties` that were saved during the `Challenge`. 
        // In a real app, this uses cookies (CookieAuthenticationHandler).
        // Since we are mocking the handler, we don't have the cookie mechanism preserving the properties.
        
        // Workaround: We can check the QueryString of the current request (the callback) and populate the Properties from it, 
        // assuming the test passes the token back in the query string when calling signin-github.
        // BUT the Controller logic is:
        // if (authenticateResult.Properties.Items.TryGetValue("invitationToken", ...))
        
        // Let's assume the test adds ?invitationToken=... to the callback URL to help us out, 
        // OR we just cheat and grab it from the query string and put it in Properties.
        
        if (Request.Query.TryGetValue("invitationToken", out var tokenQuery))
        {
            properties.Items["invitationToken"] = tokenQuery.ToString();
        }

        var ticket = new AuthenticationTicket(principal, properties, "GitHub");
        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var redirectUri = properties.RedirectUri;
        Logger.LogInformation("TestAuthenticationHandler: Initial RedirectUri from properties: {RedirectUri}", redirectUri);

        if (string.IsNullOrEmpty(redirectUri))
        {
            redirectUri = "/";
        }

        // Ensure the invitationToken is passed along in the redirect query string
        if (properties.Items.TryGetValue("invitationToken", out var token) && token != null)
        {
            var separator = redirectUri.Contains("?") ? "&" : "?";
            redirectUri += $"{separator}invitationToken={token}";
            Logger.LogInformation("TestAuthenticationHandler: Appended invitationToken. New RedirectUri: {RedirectUri}", redirectUri);
        }

        Response.Redirect(redirectUri);
        return Task.CompletedTask;
    }
}