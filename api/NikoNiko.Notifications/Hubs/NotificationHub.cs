using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using NikoNiko.Notifications.Services;

namespace NikoNiko.Notifications.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IUserConnectionManager _userConnectionManager;

        public NotificationHub(IUserConnectionManager userConnectionManager)
        {
            _userConnectionManager = userConnectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null) // Add null check for userId
            {
                _userConnectionManager.AddConnection(userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception) // Make exception nullable
        {
            _userConnectionManager.RemoveConnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string user, string message)
        {
            // This method is called by the frontend. We don't want the sender to receive the notification.
            // The actual dispatching logic will be in the NotificationsController.
            // So, this method can be removed or left as-is if not used directly for cross-user notifications from client.
            // For now, let's keep it as is, but it won't be used for the current feature.
        }
    }
}