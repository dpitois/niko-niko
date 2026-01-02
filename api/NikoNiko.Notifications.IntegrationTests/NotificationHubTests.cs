using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

using NikoNiko.Notifications;

using Xunit;

namespace NikoNiko.Notifications.IntegrationTests
{
    public class NotificationHubTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public NotificationHubTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SendNotification_ReceivesNotification()
        {
            // Arrange
            var client = _factory.CreateClient();
            var connection = new HubConnectionBuilder()
                .WithUrl($"ws://localhost/notificationHub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                })
                .Build();

            var receivedUser = "";
            var receivedMessage = "";

            connection.On<string, string>("ReceiveNotification", (user, message) =>
            {
                receivedUser = user;
                receivedMessage = message;
            });

            await connection.StartAsync();

            // Act
            var user = "TestUser";
            var message = "Hello from test!";
            await connection.InvokeAsync("SendNotification", user, message);

            // Assert
            // Give some time for the message to propagate
            await Task.Delay(500);

            Assert.Equal(user, receivedUser);
            Assert.Equal(message, receivedMessage);

            await connection.StopAsync();
        }
    }
}