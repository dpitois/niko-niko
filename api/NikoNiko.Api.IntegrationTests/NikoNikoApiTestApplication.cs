using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.IntegrationTests;

// Helper class to bootstrap the application with a test database
public class NikoNikoApiTestApplication : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection; // Made nullable to resolve CS8618
    public static bool EnableEfCoreLogging { get; set; } = false;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // Open connection to keep the in-memory DB alive

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            conf.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Authentication:Jwt:Key", "supersecretjwtkeythatisatleast32characterslong"}, // Dummy key for testing
                {"Authentication:Jwt:Issuer", "NikoNikoTestIssuer"},
                {"Authentication:Jwt:Audience", "NikoNikoTestAudience"},
                {"SignalRService:BaseUrl", "http://localhost"}, // Dummy URL for testing
                {"Authentication:FrontendRedirectUrl", "http://localhost:3000/auth/callback"} // Required for OAuth callback tests
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the app's ApplicationDbContext registration.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove existing ITokenService to register a test-specific one if needed, or ensure correct one is used
            var tokenServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
            if (tokenServiceDescriptor != null)
            {
                services.Remove(tokenServiceDescriptor);
            }
            // Register actual TokenService, it's needed for generating JWT for test users
            services.AddScoped<ITokenService, TokenService>();

            // Remove all authentication related services from the main application's configuration
            var authenticationServices = services.Where(s =>
                s.ServiceType.FullName!.Contains("Microsoft.AspNetCore.Authentication") || // Used '!' as this code has been tested for non-nullability during previous steps
                s.ServiceType.FullName!.Contains("Microsoft.Extensions.Options.IConfigureOptions`1[Microsoft.AspNetCore.Authentication") ||
                s.ServiceType.FullName!.Contains("Microsoft.Extensions.Options.IPostConfigureOptions`1[Microsoft.AspNetCore.Authentication")
            ).ToList();

            foreach (var service in authenticationServices)
            {
                services.Remove(service);
            }

            // Remove any IHostedService implementations related to authentication schemes that might try to validate options too early
            var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService)).ToList();
            foreach (var hostedService in hostedServices)
            {
                // Heuristic: remove hosted services that might be related to authentication or options validation
                if (hostedService.ImplementationType != null &&
                    (hostedService.ImplementationType.FullName?.Contains("Authentication") == true || hostedService.ImplementationType.FullName?.Contains("Options") == true))
                {
                    services.Remove(hostedService);
                }
            }

            // Re-add a simplified authentication for JWT only
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "NikoNikoTestIssuer", // Must match the one set in ConfigureAppConfiguration
                    ValidAudience = "NikoNikoTestAudience", // Must match the one set in ConfigureAppConfiguration
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("supersecretjwtkeythatisatleast32characterslong")) // Must match
                };
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("GitHub", options => { });
            // End of authentication configuration

            // Add ApplicationDbContext using an in-memory SQLite database for testing.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection); // Use the open in-memory connection
                
                if (EnableEfCoreLogging)
                {
                    options.EnableSensitiveDataLogging(); // For better debugging
                    options.EnableDetailedErrors(); // For better debugging
                }
                else
                {
                    // Suppress command execution logs to reduce verbosity
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted));
                }
            });

            // Add any other test-specific services here.
            // Ensure ITeamInvitationService is registered (it should be from NikoNiko.Api)
            // services.AddScoped<ITeamInvitationService, TeamInvitationService>();
        });

        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseSetting("ConnectionStrings:DefaultConnection", _connection.ConnectionString);
            webHostBuilder.ConfigureTestServices(services =>
            {
                // Remove the original DataProtection configuration if it exists
                var dataProtectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDataProtectionProvider));
                if (dataProtectionDescriptor != null)
                {
                    services.Remove(dataProtectionDescriptor);
                }
                // Add the ephemeral DataProtection provider for testing
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
            });
        });

        var host = base.CreateHost(builder);

        // Ensure the database is created and migrated for every test run.
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted(); // Ensure a clean state before each test run
            dbContext.Database.EnsureCreated(); // This will use the model to create the schema
        }

        return host;
    }

    public async Task<(User user, HttpClient client, string jwtToken)> CreateUserAndClient(string name, bool isSuperAdmin = false)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            OAuthId = $"github|{name.ToLower().Replace(" ", "")}",
            Email = $"{name.ToLower().Replace(" ", "")}@example.com",
            IsSuperAdmin = isSuperAdmin
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var client = CreateClient();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        if (user.IsSuperAdmin)
        {
            claims.Add(new Claim("is_super_admin", "true"));
        }

        var jwtToken = tokenService.GenerateToken(claims.ToArray());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        return (user, client, jwtToken);
    }

    public async Task<Team> CreateTeam(string name, Guid adminId)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var team = new Team { Id = Guid.NewGuid(), Name = name, AdminId = adminId };
        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync();
        return team;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close(); // Add null conditional operator
    }
}
