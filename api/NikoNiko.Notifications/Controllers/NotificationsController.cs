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
        public async Task<IActionResult> DispatchNotification([FromBody] NotificationPayload payload)
        {
            if (string.IsNullOrEmpty(payload.User) || string.IsNullOrEmpty(payload.Message))
            {
                return BadRequest("User and Message cannot be empty.");
            }

            var connectionIdsToExclude = _userConnectionManager.GetConnections(payload.UserId)?.ToArray() ?? new string[0];
            await _hubContext.Clients.AllExcept(connectionIdsToExclude).SendAsync("ReceiveNotification", payload.User, payload.Message);
            return Ok();
        }
    }

    public class NotificationPayload
    {
        public string User { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }
}