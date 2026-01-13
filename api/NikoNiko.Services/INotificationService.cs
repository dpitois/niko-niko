using System.Threading.Tasks;

namespace NikoNiko.Services
{
    public interface INotificationService
    {
        Task SendMoodNotificationAsync(string user, string message, string userId);
        Task NotifyTeamRenamedAsync(Guid teamId, string newName);
    }
}