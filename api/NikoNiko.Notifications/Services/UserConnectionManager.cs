using System.Collections.Generic;
using System.Linq;

namespace NikoNiko.Notifications.Services
{
    public class UserConnectionManager : IUserConnectionManager
    {
        private static readonly Dictionary<string, HashSet<string>> _userConnectionMap = new Dictionary<string, HashSet<string>>();
        private static readonly object _lock = new object();

        public void AddConnection(string userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_userConnectionMap.ContainsKey(userId))
                {
                    _userConnectionMap[userId] = new HashSet<string>();
                }
                _userConnectionMap[userId].Add(connectionId);
            }
        }

        public void RemoveConnection(string connectionId)
        {
            lock (_lock)
            {
                foreach (var userId in _userConnectionMap.Keys)
                {
                    if (_userConnectionMap[userId].Contains(connectionId))
                    {
                        _userConnectionMap[userId].Remove(connectionId);
                        if (_userConnectionMap[userId].Count == 0)
                        {
                            _userConnectionMap.Remove(userId);
                        }
                        break;
                    }
                }
            }
        }

        public HashSet<string> GetConnections(string userId)
        {
            lock (_lock)
            {
                return _userConnectionMap.GetValueOrDefault(userId);
            }
        }
    }
}