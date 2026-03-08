using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.Models;

namespace NikoNiko.Core.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamWithSprintsDto>> GetTeamsAsync(Guid userId, bool isSuperAdmin);
    Task<TeamWithSprintsDto?> GetTeamByIdAsync(Guid teamId);
    Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto, Guid adminId, bool isSuperAdmin);
    Task UpdateTeamAsync(Guid teamId, UpdateTeamDto updateTeamDto);
    Task TransferAdminAsync(Guid teamId, Guid newAdminId);
    Task DeleteTeamAsync(Guid teamId);
    Task RemoveUserFromTeamAsync(Guid teamId, Guid userId);
    Task<Team> CreateDefaultTeamForUserAsync(User user);
}