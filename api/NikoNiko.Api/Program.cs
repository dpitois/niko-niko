using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions; // Added for Regex

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NikoNiko.Api.Authorization;
using NikoNiko.Core.Interfaces;
using NikoNiko.Data;
using NikoNiko.Data.PostgreSql;
using NikoNiko.Data.Sqlite;
using NikoNiko.Services;

using IAuthorizationHandler = Microsoft.AspNetCore.Authorization.IAuthorizationHandler;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// 1. Add services to the container.
// -----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITeamInvitationService, TeamInvitationService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<IMoodService, MoodService>();

// Add HttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add custom authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, IsTeamAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsTeamMemberHandler>();

// Add HttpClient for SignalR Service communication
builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
{
    client.BaseAddress = new Uri(config["SignalRService:BaseUrl"]!);
});

// Configure DbContext based on the provider specified in appsettings.json
var dbProvider = config["DatabaseProvider"];
if (dbProvider == "SQLite")
{
    builder.Services.AddSqlitePersistence(config);
}
else
{
    builder.Services.AddPostgreSqlPersistence(config);
}

// Configure Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

// Configure Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Only loopback proxies are trusted by default.
    // Clear that default and add the proxy that is running in docker-compose.
    // In a production environment, you might specify known proxies/networks.
    // For a typical Docker setup, trusting all is often necessary due to dynamic IPs.
    // Be cautious with this in a highly sensitive environment.
    options.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("0.0.0.0"), 0)); // Trust all networks
    options.KnownProxies.Add(IPAddress.Parse("0.0.0.0")); // Trust all proxies
});

// Configure Https Redirection
if (builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
        options.HttpsPort = 443;
    });
}

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("ExternalCookie")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Authentication:Jwt:Issuer"],
            ValidAudience = config["Authentication:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:Jwt:Key"]!))
        };
    })
    .AddGitHub(options =>
    {
        options.SignInScheme = "ExternalCookie";
        options.ClientId = config["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = config["Authentication:GitHub:ClientSecret"]!;
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");
        options.ClaimActions.MapJsonKey("urn:github:avatar_url", "avatar_url");
        options.Events.OnRemoteFailure = context =>
        {
            var failureMessage = Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error");
            context.Response.Redirect(config["Authentication:FrontendRedirectUrl"] + "/login?error=" + failureMessage);
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });

var googleClientId = config["Authentication:Google:ClientId"];
var googleClientSecret = config["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.SignInScheme = "ExternalCookie";
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.Events.OnRemoteFailure = context =>
        {
            var failureMessage = Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error");
            context.Response.Redirect(config["Authentication:FrontendRedirectUrl"] + "/login?error=" + failureMessage);
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}

var discordClientId = config["Authentication:Discord:ClientId"];
var discordClientSecret = config["Authentication:Discord:ClientSecret"];

if (!string.IsNullOrEmpty(discordClientId) && !string.IsNullOrEmpty(discordClientSecret))
{
    builder.Services.AddAuthentication().AddDiscord(options =>
    {
        options.SignInScheme = "ExternalCookie";
        options.ClientId = discordClientId;
        options.ClientSecret = discordClientSecret;
        options.CallbackPath = "/signin-discord";
        options.Prompt = "none";
        options.Scope.Add("openid");
        options.ClaimActions.MapJsonKey("urn:discord:avatar:hash", "avatar");
        options.Events.OnRemoteFailure = context =>
        {
            // Check for 'interaction_required' error which means 'prompt=none' failed
            if (context.Failure?.Message?.Contains("interaction_required", StringComparison.OrdinalIgnoreCase) == true ||
                context.Request.Query["error"] == "interaction_required")
            {
                // Fallback: User needs to consent. 
                // CRITICAL: Preserve the invitationToken if it was present in the original challenge
                var redirectUrl = "/api/auth/login-discord?prompt=consent";
                if (context.Properties?.Items.TryGetValue("invitationToken", out var token) == true && !string.IsNullOrEmpty(token))
                {
                    redirectUrl += $"&invitationToken={Uri.EscapeDataString(token)}";
                }

                context.Response.Redirect(redirectUrl);
                context.HandleResponse();
                return Task.CompletedTask;
            }

            var failureMessage = Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error");
            context.Response.Redirect(config["Authentication:FrontendRedirectUrl"] + "/login?error=" + failureMessage);
            context.HandleResponse();
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            // Allow overriding the default 'prompt=none' if specified in AuthenticationProperties
            if (context.Properties.Items.TryGetValue("prompt", out var prompt) && !string.IsNullOrEmpty(prompt))
            {
                var uri = context.RedirectUri;
                // Robustly remove existing prompt param
                uri = Regex.Replace(uri, @"&prompt=[^&]*", "", RegexOptions.IgnoreCase);
                uri = Regex.Replace(uri, @"\?prompt=[^&]*&", "?", RegexOptions.IgnoreCase);
                uri = Regex.Replace(uri, @"\?prompt=[^&]*$", "", RegexOptions.IgnoreCase);

                // Append new prompt
                var separator = uri.Contains("?") ? "&" : "?";
                uri += $"{separator}prompt={Uri.EscapeDataString(prompt)}";

                context.RedirectUri = uri;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
}

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_super_admin", "true"));

    options.AddPolicy("IsTeamAdmin", policy =>
        policy.Requirements.Add(new IsTeamAdminRequirement()));

    options.AddPolicy("IsTeamMember", policy =>
        policy.Requirements.Add(new IsTeamMemberRequirement()));
});

// Add services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Also include XML comments from Core project if they exist
    var coreXmlFilename = "NikoNiko.Core.xml";
    var coreXmlPath = Path.Combine(AppContext.BaseDirectory, coreXmlFilename);
    if (File.Exists(coreXmlPath))
    {
        options.IncludeXmlComments(coreXmlPath);
    }
});

builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();
}

// Apply migrations on startup and sync roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

await SeedAndSyncSuperAdminRoles(app);


// 2. Configure the HTTP request pipeline.
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

// Configure Https Redirection
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------------
// Helper method to seed and synchronize super admin roles on startup
async Task SeedAndSyncSuperAdminRoles(WebApplication webApp)
{
    using var scope = webApp.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var superAdminEmailsConfig = configuration["SUPER_ADMINS"];
    var superAdminEmails = !string.IsNullOrWhiteSpace(superAdminEmailsConfig)
        ? superAdminEmailsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        : new List<string>();

    if (superAdminEmails.Any())
    {
        // Env var method: Sync roles based on the email list
        var allUsers = await dbContext.Users.ToListAsync();

        // First, demote all current super admins to handle removals
        foreach (var user in allUsers.Where(u => u.IsSuperAdmin))
        {
            user.IsSuperAdmin = false;
        }

        // Then, promote users from the list
        var usersToPromote = allUsers.Where(u => !string.IsNullOrEmpty(u.Email) && superAdminEmails.Contains(u.Email, StringComparer.OrdinalIgnoreCase));
        foreach (var user in usersToPromote)
        {
            user.IsSuperAdmin = true;
        }
    }
    else
    {
        // Fallback method: Ensure the first user ever created is an admin if no one else is
        var isAnyAdmin = await dbContext.Users.AnyAsync(u => u.IsSuperAdmin);
        if (!isAnyAdmin)
        {
            var firstUser = await dbContext.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
            if (firstUser != null)
            {
                firstUser.IsSuperAdmin = true;
            }
        }
    }

    await dbContext.SaveChangesAsync();
}

// -----------------------------------------------------------------------------
app.Run();