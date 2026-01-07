using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers; // Added
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using NikoNiko.Notifications;
using NikoNiko.Notifications.Services;

using Xunit;

namespace NikoNiko.Notifications.IntegrationTests
{
    public class NotificationHubTests : IClassFixture<NotificationTestApplication>
    {
        private readonly NotificationTestApplication _factory;

        public NotificationHubTests(NotificationTestApplication factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Dispatch_ShouldNotNotifySender_ButShouldNotifyOthers()
        {
            // Arrange
            var senderId = "senderUser";
            var receiverId = "receiverUser";

            var senderToken = GenerateTestJwtToken(senderId, _factory.Services.GetRequiredService<IConfiguration>());
            var receiverToken = GenerateTestJwtToken(receiverId, _factory.Services.GetRequiredService<IConfiguration>());

            var senderReceived = false;
            var receiverTcs = new TaskCompletionSource<bool>();

            // Setup sender connection
            var senderConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, "notificationHub"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.AccessTokenProvider = () => Task.FromResult<string?>(senderToken);
                })
                .Build();
            
            senderConnection.On<string, string>("ReceiveNotification", (user, message) =>
            {
                senderReceived = true;
            });

            // Setup receiver connection
            var receiverConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, "notificationHub"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.AccessTokenProvider = () => Task.FromResult<string?>(receiverToken);
                })
                .Build();

            receiverConnection.On<string, string>("ReceiveNotification", (user, message) =>
            {
                receiverTcs.SetResult(true);
            });

            await senderConnection.StartAsync();
            await receiverConnection.StartAsync();

            // Act: Dispatch a notification as the 'sender'
            var httpClient = _factory.CreateClient();
            var payload = new
            {
                User = "Test Message User",
                Message = "This is a test message.",
                UserId = senderId // Critically, we identify the sender by their UserId
            };
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/api/Notifications/dispatch", content);
            response.EnsureSuccessStatusCode();

            // Assert
            var receiverTask = receiverTcs.Task;
            var completedTask = await Task.WhenAny(receiverTask, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.True(receiverTask.IsCompletedSuccessfully, "Receiver should have received the notification.");
            Assert.False(senderReceived, "Sender should not have received their own notification.");

            // Cleanup
            await senderConnection.StopAsync();
            await receiverConnection.StopAsync();
        }

        // Helper method to generate a JWT token for testing
        private string GenerateTestJwtToken(string userId, IConfiguration config)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Email, $"{userId}@example.com")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = config["Authentication:Jwt:Issuer"],
                Audience = config["Authentication:Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    // Custom WebApplicationFactory for Notification Tests
    public class NotificationTestApplication : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Authentication:Jwt:Key", "supersecretnotificationjwtkeythatisatleast32characterslong"}, // Dummy key
                    {"Authentication:Jwt:Issuer", "NotificationTestIssuer"},
                    {"Authentication:Jwt:Audience", "NotificationTestAudience"}
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing IHostedService authentication related registrations
                var hostedServices = services.Where(s => s.ServiceType == typeof(IHostedService)).ToList();
                foreach (var hostedService in hostedServices)
                {
                    if (hostedService.ImplementationType != null &&
                        (hostedService.ImplementationType.FullName?.Contains("Authentication") == true || hostedService.ImplementationType.FullName?.Contains("Options") == true))
                    {
                        services.Remove(hostedService);
                    }
                }

                // Remove all authentication related services from the main application's configuration
                var authenticationServices = services.Where(s =>
                    s.ServiceType.FullName!.Contains("Microsoft.AspNetCore.Authentication") ||
                    s.ServiceType.FullName!.Contains("Microsoft.Extensions.Options.IConfigureOptions`1[Microsoft.AspNetCore.Authentication") ||
                    s.ServiceType.FullName!.Contains("Microsoft.Extensions.Options.IPostConfigureOptions`1[Microsoft.AspNetCore.Authentication")
                ).ToList();

                foreach (var service in authenticationServices)
                {
                    services.Remove(service);
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
                        ValidIssuer = "NotificationTestIssuer", // Must match the one set in ConfigureAppConfiguration
                        ValidAudience = "NotificationTestAudience", // Must match the one set in ConfigureAppConfiguration
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("supersecretnotificationjwtkeythatisatleast32characterslong")) // Must match
                    };
                });
                // Since this factory tests the Notification service, it will use its own Program.cs's services.
                // We don't need to explicitly add DbContext or other services here unless overriding them.
            });

            var host = base.CreateHost(builder);
            return host;
        }
    }
}