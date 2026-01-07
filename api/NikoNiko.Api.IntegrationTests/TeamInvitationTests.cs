using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text; // Added for Encoding
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer; // Added for JwtBearerDefaults
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite; // Needed for SqliteConnection
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Added for AddInMemoryCollection
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens; // Added for TokenValidationParameters and SymmetricSecurityKey

using NikoNiko.Api;
using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

using Xunit;

namespace NikoNiko.Api.IntegrationTests;

public class TeamInvitationTests
{
    [Fact]
    public async Task AcceptInvitation_WhenTokenIsValid_ShouldAddUserToTeamAndInvalidateInvitation()
    {
        // Arrange
        await using var application = new NikoNikoApiTestApplication();

        // 1. Setup initial data
        var teamAdmin = new User { Id = Guid.NewGuid(), Name = "Admin User", OAuthId = "github|admin", Email = "admin@example.com" };
        var newMember = new User { Id = Guid.NewGuid(), Name = "New Member", OAuthId = "github|newmember", Email = "newmember@example.com" };
        var team = new Team { Id = Guid.NewGuid(), Name = "Test Team", AdminId = teamAdmin.Id };

        // Use a separate scope to seed data and ensure it's saved before the next step
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.AddRange(teamAdmin, newMember);
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }

        // 2. Create an invitation
        string invitationToken;
        Guid invitationId;
        using (var scope = application.Services.CreateScope())
        {
            var invitationService = scope.ServiceProvider.GetRequiredService<ITeamInvitationService>();
            var createDto = new CreateTeamInvitationDto { TeamId = team.Id, ExpirationInDays = 1 };
            var invitationDto = await invitationService.CreateTeamInvitationAsync(team.Id, teamAdmin.Id, createDto);
            invitationToken = invitationDto.Token;
            invitationId = invitationDto.Id;
        }

        // 3. Create a client authenticated as the new member
        var client = application.CreateClient();
        string jwtToken;
        using (var scope = application.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, newMember.Id.ToString()) };
            jwtToken = tokenService.GenerateToken(claims);
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        // Act
        var response = await client.PostAsync($"/api/teaminvitations/{invitationToken}/accept", null);

        // Assert
        response.EnsureSuccessStatusCode(); // Status 2xx

        // Verify the database state
        using (var scope = application.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Assertion 1: User is now a member of the team
            var isMember = await dbContext.TeamUsers.AnyAsync(tu => tu.TeamId == team.Id && tu.UserId == newMember.Id);
            Assert.True(isMember, "User should have been added to the team.");

            // Assertion 2: Invitation is invalidated
            var invitation = await dbContext.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(ti => ti.Id == invitationId);
            Assert.NotNull(invitation);
            // The service sets Status to "Accepted". The bug was that IsDeleted was not set.
            Assert.Equal("Accepted", invitation.Status);
            // This is the key assertion for the bug fix
            Assert.True(invitation.IsDeleted, "Invitation should be marked as soft-deleted (IsDeleted = true).");
        }
    }
}


// Helper class to bootstrap the application with a test database
public class NikoNikoApiTestApplication : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection; // Made nullable to resolve CS8618

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
                        {"Authentication:Jwt:Audience", "NikoNikoTestAudience"}
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
            });
            // End of authentication configuration

            // Add ApplicationDbContext using an in-memory SQLite database for testing.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection); // Use the open in-memory connection
                options.EnableSensitiveDataLogging(); // For better debugging
                options.EnableDetailedErrors(); // For better debugging
            });

            // Add any other test-specific services here.
            // Ensure ITeamInvitationService is registered (it should be from NikoNiko.Api)
            // services.AddScoped<ITeamInvitationService, TeamInvitationService>();
        });

        builder.ConfigureWebHost(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connection.ConnectionString);
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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close(); // Add null conditional operator
    }
}