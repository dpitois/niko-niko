using NikoNiko.Core.Models;

namespace NikoNiko.Core.Interfaces;

public interface ITeamService
{
    Task<Team> CreateDefaultTeamForUserAsync(User user);
}