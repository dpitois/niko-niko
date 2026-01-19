using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using NikoNiko.Notifications.Hubs;
using NikoNiko.Notifications.Services;

namespace NikoNiko.Notifications.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserConnectionManager _userConnectionManager;

        public NotificationsController(IHubContext<NotificationHub> hubContext, IUserConnectionManager userConnectionManager)
        {
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
        }

        [HttpPost("dispatch")]
        public async Task<IActionResult> Dispatch([FromBody] NotificationPayload payload)
        {
            if (payload.Type == "TeamRenamed" && payload.TeamId.HasValue)
            {
                await _hubContext.Clients.Group(payload.TeamId.Value.ToString()).SendAsync("ReceiveTeamRenamed", payload.TeamId, payload.NewName);
                return Ok();
            }

            if (string.IsNullOrEmpty(payload.User) || string.IsNullOrEmpty(payload.Message) || string.IsNullOrEmpty(payload.UserId))
            {
                return BadRequest("User, Message, and UserId cannot be empty.");
            }

            if (payload.TeamId.HasValue)
            {
                // Send to team group, excluding sender
                var connectionIdsToExclude = _userConnectionManager.GetConnections(payload.UserId)?.ToArray() ?? new string[0];
                await _hubContext.Clients.GroupExcept(payload.TeamId.Value.ToString(), connectionIdsToExclude).SendAsync("ReceiveNotification", payload.User, payload.Message);
            }
            else
            {
                // Fallback: Send to all (should likely be avoided in production for privacy, but keeping for legacy compatibility or global announcements)
                var connectionIdsToExclude = _userConnectionManager.GetConnections(payload.UserId)?.ToArray() ?? new string[0];
                await _hubContext.Clients.AllExcept(connectionIdsToExclude).SendAsync("ReceiveNotification", payload.User, payload.Message);
            }

            return Ok();
        }

        [HttpPost("manage-groups")]
        public async Task<IActionResult> ManageGroups([FromBody] GroupManagementPayload payload)
        {
            if (string.IsNullOrEmpty(payload.UserId) || payload.TeamId == Guid.Empty)
            {
                return BadRequest("UserId and TeamId are required.");
            }

            var connectionIds = _userConnectionManager.GetConnections(payload.UserId);
            if (connectionIds == null || !connectionIds.Any())
            {
                return Ok("User has no active connections.");
            }

            foreach (var connectionId in connectionIds)
            {
                if (payload.Action == "Add")
                {
                    await _hubContext.Groups.AddToGroupAsync(connectionId, payload.TeamId.ToString());
                }
                else if (payload.Action == "Remove")
                {
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, payload.TeamId.ToString());
                }
            }

            return Ok();
        }
    }

    public class NotificationPayload
    {
        public string? Type { get; set; }
        public Guid? TeamId { get; set; }
        public string? NewName { get; set; }
        public string? User { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
    }

    public class GroupManagementPayload
    {
        public string UserId { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public string Action { get; set; } = "Add"; // "Add" or "Remove"
    }
}