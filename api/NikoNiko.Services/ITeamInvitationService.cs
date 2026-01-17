using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;

namespace NikoNiko.Services
{
    public interface ITeamInvitationService
    {
        Task<TeamInvitationDto> CreateTeamInvitationAsync(Guid teamId, Guid creatorUserId, CreateTeamInvitationDto createDto, bool isSuperAdmin = false);
        Task<TeamInvitationDto> AcceptTeamInvitationAsync(string token, Guid acceptedByUserId);
        Task<IEnumerable<TeamInvitationDto>> GetTeamInvitationsAsync(Guid teamId, Guid requestingUserId);
        Task<TeamInvitation?> GetTeamInvitationByTokenAsync(string token);
        Task DeleteTeamInvitationAsync(Guid invitationId, Guid requestingUserId, bool isSuperAdmin = false);
    }
}