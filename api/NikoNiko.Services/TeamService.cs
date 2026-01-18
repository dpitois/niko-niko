using Microsoft.Extensions.Logging;

using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeamService> _logger;

    public TeamService(ApplicationDbContext context, ILogger<TeamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Team> CreateDefaultTeamForUserAsync(User user)
    {
        var teamName = $"{(string.IsNullOrWhiteSpace(user.Name) ? "My" : user.Name)}'s Team";
        var newTeam = new Team
        {
            Name = teamName,
            AdminId = user.Id
        };

        var teamUser = new TeamUser
        {
            Team = newTeam,
            UserId = user.Id
        };

        _context.Teams.Add(newTeam);
        _context.TeamUsers.Add(teamUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auto-created team {TeamName} for user {UserId}.", teamName, user.Id);

        return newTeam;
    }
}