using System.Collections.Generic;

namespace NikoNiko.Notifications.Services
{
    public interface IUserConnectionManager
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string connectionId);
        HashSet<string> GetConnections(string userId);
    }
}