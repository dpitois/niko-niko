using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NikoNiko.Core.Interfaces;

namespace NikoNiko.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendMoodNotificationAsync(string user, string message, string userId, Guid teamId)
        {
            try
            {
                var payload = new
                {
                    User = user,
                    Message = message,
                    UserId = userId,
                    TeamId = teamId
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/notifications/dispatch", content);

                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code
                _logger.LogInformation("Mood notification sent to SignalR service successfully.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error sending mood notification to SignalR service.");
            }
        }

        public async Task NotifyTeamRenamedAsync(Guid teamId, string newName)
        {
            try
            {
                var payload = new
                {
                    Type = "TeamRenamed",
                    TeamId = teamId,
                    NewName = newName
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/notifications/dispatch", content);

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Team renamed notification sent to SignalR service successfully.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error sending team renamed notification to SignalR service.");
            }
        }

        public async Task UpdateUserGroupAsync(string userId, Guid teamId, bool isJoining)
        {
            try
            {
                var payload = new
                {
                    UserId = userId,
                    TeamId = teamId,
                    Action = isJoining ? "Add" : "Remove"
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/notifications/manage-groups", content);

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Group management request ({Action}) sent to SignalR service successfully for user {UserId}.", isJoining ? "Add" : "Remove", userId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error sending group management request to SignalR service.");
            }
        }
    }
}