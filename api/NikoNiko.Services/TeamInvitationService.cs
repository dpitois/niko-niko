using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services
{
    public class TeamInvitationService : ITeamInvitationService
    {
        private readonly ApplicationDbContext _context;

        public TeamInvitationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamInvitationDto> CreateTeamInvitationAsync(Guid teamId, Guid creatorUserId, CreateTeamInvitationDto createDto, bool isSuperAdmin = false)
        {
            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .ThenInclude(tu => tu.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                throw new KeyNotFoundException($"Team with ID {teamId} not found.");
            }

            var creatorUser = await _context.Users.FindAsync(creatorUserId);
            if (creatorUser == null)
            {
                throw new KeyNotFoundException($"Creator user with ID {creatorUserId} not found.");
            }

            // Check if the creator is an admin of the team or super admin
            if (team.AdminId != creatorUserId && !isSuperAdmin)
            {
                throw new UnauthorizedAccessException("Only team admins can create invitations.");
            }

            var token = GenerateUniqueInvitationToken();
            var expirationDate = DateTime.UtcNow.AddDays(createDto.ExpirationInDays);

            var invitation = new TeamInvitation
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                CreatorUserId = creatorUserId,
                Token = token,
                ExpirationDate = expirationDate,
                Status = "Pending"
            };

            _context.TeamInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return new TeamInvitationDto
            {
                Id = invitation.Id,
                TeamId = invitation.TeamId,
                TeamName = team.Name,
                CreatorUserId = invitation.CreatorUserId,
                CreatorUserName = creatorUser.Name,
                ExpirationDate = invitation.ExpirationDate,
                Token = invitation.Token,
                Status = invitation.Status
            };
        }

        public async Task<TeamInvitationDto> AcceptTeamInvitationAsync(string token, Guid acceptedByUserId)
        {
            var invitation = await _context.TeamInvitations
                .Include(ti => ti.Team)
                .Include(ti => ti.CreatorUser)
                .FirstOrDefaultAsync(ti => ti.Token == token);

            if (invitation == null)
            {
                throw new KeyNotFoundException("Invitation not found or invalid.");
            }

            if (invitation.Status != "Pending")
            {
                throw new InvalidOperationException("Invitation has already been used or is not pending.");
            }

            if (invitation.ExpirationDate <= DateTime.UtcNow)
            {
                invitation.Status = "Expired";
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Invitation has expired.");
            }

            var acceptedByUser = await _context.Users.FindAsync(acceptedByUserId);
            if (acceptedByUser == null)
            {
                throw new KeyNotFoundException($"User with ID {acceptedByUserId} not found.");
            }

            // Check if the user is already part of the team
            var existingTeamUser = await _context.TeamUsers.FirstOrDefaultAsync(tu => tu.TeamId == invitation.TeamId && tu.UserId == acceptedByUserId);

            if (existingTeamUser == null)
            {
                // If user is not already a member, add them
                _context.TeamUsers.Add(new TeamUser { TeamId = invitation.TeamId, UserId = acceptedByUserId });
            }
            // If user is already a member, do nothing to TeamUsers, just proceed with invitation invalidation.
            // This prevents throwing an error if they are already a member, which is fine for the invitation's purpose.

            // Invalidate the invitation after acceptance, regardless if they were already a member or just added.
            invitation.Status = "Accepted";
            invitation.AcceptedByUserId = acceptedByUserId;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.IsDeleted = true; // Mark as soft-deleted after acceptance

            await _context.SaveChangesAsync();

            return new TeamInvitationDto
            {
                Id = invitation.Id,
                TeamId = invitation.TeamId,
                TeamName = invitation.Team!.Name,
                CreatorUserId = invitation.CreatorUserId,
                CreatorUserName = invitation.CreatorUser!.Name,
                ExpirationDate = invitation.ExpirationDate,
                Token = invitation.Token,
                Status = invitation.Status
            };
        }

        public async Task<IEnumerable<TeamInvitationDto>> GetTeamInvitationsAsync(Guid teamId, Guid requestingUserId)
        {
            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .ThenInclude(tu => tu.User)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                throw new KeyNotFoundException($"Team with ID {teamId} not found.");
            }

            // Check if the requesting user is an admin of the team
            if (team.AdminId != requestingUserId)
            {
                throw new UnauthorizedAccessException("Only team admins can view invitations.");
            }

            var invitations = await _context.TeamInvitations
                .Where(ti => ti.TeamId == teamId)
                .Include(ti => ti.Team)
                .Include(ti => ti.CreatorUser)
                .OrderByDescending(ti => ti.CreatedAt)
                .Select(ti => new TeamInvitationDto
                {
                    Id = ti.Id,
                    TeamId = ti.TeamId,
                    TeamName = ti.Team!.Name,
                    CreatorUserId = ti.CreatorUserId,
                    CreatorUserName = ti.CreatorUser!.Name,
                    ExpirationDate = ti.ExpirationDate,
                    Token = ti.Token,
                    Status = ti.Status
                })
                .ToListAsync();

            return invitations;
        }

        public async Task<TeamInvitation?> GetTeamInvitationByTokenAsync(string token)
        {
            return await _context.TeamInvitations
                .Include(ti => ti.Team)
                .Include(ti => ti.CreatorUser)
                .FirstOrDefaultAsync(ti => ti.Token == token);
        }

        public async Task DeleteTeamInvitationAsync(Guid invitationId, Guid requestingUserId, bool isSuperAdmin = false)
        {
            var invitation = await _context.TeamInvitations
                .IgnoreQueryFilters() // Ignorer le filtre global pour récupérer l'invitation, même si elle est déjà soft-deleted
                .Include(ti => ti.Team)
                .FirstOrDefaultAsync(ti => ti.Id == invitationId);

            if (invitation == null)
            {
                throw new KeyNotFoundException($"Invitation with ID {invitationId} not found.");
            }

            if (!isSuperAdmin && invitation.Team?.AdminId != requestingUserId)
            {
                throw new UnauthorizedAccessException("Only team admins or super admins can delete invitations.");
            }

            invitation.IsDeleted = true; // Marque l'invitation comme supprimée logiquement
            await _context.SaveChangesAsync();
        }


        private string GenerateUniqueInvitationToken()
        {
            const int tokenLength = 32; // You can adjust the length as needed
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[tokenLength];
                rng.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}