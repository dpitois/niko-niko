using System.Threading.Tasks;

namespace NikoNiko.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendMoodNotificationAsync(string user, string message, string userId, Guid teamId);
        Task NotifyTeamRenamedAsync(Guid teamId, string newName);
        Task UpdateUserGroupAsync(string userId, Guid teamId, bool isJoining);
    }
}