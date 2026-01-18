using NikoNiko.Core.Models;

namespace NikoNiko.Services;

public interface ITeamService
{
    Task<Team> CreateDefaultTeamForUserAsync(User user);
}