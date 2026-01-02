using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

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

        public async Task SendMoodNotificationAsync(string user, string message, string userId)
        {
            try
            {
                var payload = new
                {
                    User = user,
                    Message = message,
                    UserId = userId
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
    }
}