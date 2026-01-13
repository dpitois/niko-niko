using System.Security.Claims;

using HotChocolate.Authorization;

using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.Team;
using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.GraphQL.Mutations;

public class Mutation
{
    private Guid GetUserId(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null) throw new GraphQLException("User not authenticated.");

        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
        {
            var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                var dbUser = context.Users.FirstOrDefault(u => u.Email == email);
                if (dbUser != null) return dbUser.Id;
            }
            throw new GraphQLException("User ID not found.");
        }

        if (Guid.TryParse(userIdString, out var id)) return id;
        throw new GraphQLException("Invalid User ID format.");
    }

    private bool IsSuperAdmin(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.User.HasClaim("is_super_admin", "true") ?? false;
    }

    [Authorize]
    public async Task<Team> CreateTeam(
        CreateTeamDto input,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        if (!IsSuperAdmin(httpContextAccessor))
        {
            throw new GraphQLException("Only Super Admins can create teams.");
        }

        var userId = GetUserId(httpContextAccessor, context);

        var team = new Team
        {
            Name = input.Name,
            AdminId = userId
        };

        var teamUser = new TeamUser
        {
            Team = team,
            UserId = userId
        };

        context.Teams.Add(team);
        context.TeamUsers.Add(teamUser);
        await context.SaveChangesAsync();

        return team;
    }

    [Authorize]
    public async Task<Team> UpdateTeam(
        Guid teamId,
        UpdateTeamDto input,
        ApplicationDbContext context,
        [Service] INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        var team = await context.Teams.FindAsync(teamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var userId = GetUserId(httpContextAccessor, context);
        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && team.AdminId != userId)
        {
            throw new GraphQLException("You are not authorized to update this team.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new GraphQLException("Team name cannot be empty.");
        }

        team.Name = input.Name;
        await context.SaveChangesAsync();

        await notificationService.NotifyTeamRenamedAsync(team.Id, team.Name);

        return team;
    }

    [Authorize]
    public async Task<bool> DeleteTeam(
        Guid teamId,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var team = await context.Teams.FindAsync(teamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var userId = GetUserId(httpContextAccessor, context);
        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && team.AdminId != userId)
        {
            throw new GraphQLException("You are not authorized to delete this team.");
        }

        context.Teams.Remove(team);
        await context.SaveChangesAsync();

        return true;
    }

    [Authorize]
    public async Task<bool> RemoveUserFromTeam(
        Guid teamId,
        Guid userIdToRemove,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var team = await context.Teams.FindAsync(teamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var currentUserId = GetUserId(httpContextAccessor, context);
        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && team.AdminId != currentUserId)
        {
            throw new GraphQLException("You are not authorized to remove users from this team.");
        }

        if (team.AdminId == userIdToRemove)
        {
            throw new GraphQLException("Cannot remove the team's administrator.");
        }

        var teamUser = await context.TeamUsers
            .FirstOrDefaultAsync(tu => tu.TeamId == teamId && tu.UserId == userIdToRemove);

        if (teamUser == null)
        {
            throw new GraphQLException("User is not a member of this team.");
        }

        context.TeamUsers.Remove(teamUser);
        await context.SaveChangesAsync();

        return true;
    }

    [Authorize]
    public async Task<MoodEntry> AddMoodEntry(
        CreateMoodEntryDto input,
        ApplicationDbContext context,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserId(httpContextAccessor, context);

        if (input.UserId != userId)
        {
            throw new GraphQLException("You can only create mood entries for yourself.");
        }

        var sprint = await context.Sprints.FindAsync(input.SprintId);
        if (sprint == null)
        {
            throw new GraphQLException("Sprint not found.");
        }

        var entryDate = input.Date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;
        var userLocalNow = DateTime.UtcNow.AddMinutes(input.TimezoneOffset);

        if (entryDate > userLocalNow.Date)
        {
            throw new GraphQLException("Mood entry date cannot be in the future (relative to your local time).");
        }

        if (entryDate < sprint.StartDate.Date)
        {
            throw new GraphQLException("Mood entry date cannot be before the sprint start date.");
        }

        if (entryDate > sprint.EndDate.Date)
        {
            throw new GraphQLException("Mood entry date cannot be after the sprint end date.");
        }

        var existingEntry = await context.MoodEntries.FirstOrDefaultAsync(me =>
            me.UserId == input.UserId &&
            me.SprintId == input.SprintId &&
            me.Date.Date == entryDate);

        var dbUserForNotif = await context.Users.FirstOrDefaultAsync(u => u.Id == input.UserId);
        var userEmail = dbUserForNotif?.Email ?? "Unknown User";
        var notificationMessage = $"L'utilisateur {userEmail} vient de renseigner son humeur!";

        if (existingEntry != null)
        {
            existingEntry.Mood = input.Mood;
            context.MoodEntries.Update(existingEntry);
            await context.SaveChangesAsync();

            await notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, input.UserId.ToString());

            return existingEntry;
        }
        else
        {
            var moodEntry = new MoodEntry
            {
                UserId = input.UserId,
                SprintId = input.SprintId,
                Mood = input.Mood,
                Date = entryDate
            };

            context.MoodEntries.Add(moodEntry);
            await context.SaveChangesAsync();

            await notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, input.UserId.ToString());

            return moodEntry;
        }
    }

    [Authorize]
    public async Task<Sprint> CreateSprint(
        CreateSprintDto input,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var team = await context.Teams.FindAsync(input.TeamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var userId = GetUserId(httpContextAccessor, context);
        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && team.AdminId != userId)
        {
            throw new GraphQLException("Only team admins can create sprints.");
        }

        if (input.EndDate <= input.StartDate)
        {
            throw new GraphQLException("End date must be after start date.");
        }

        var sprint = new Sprint
        {
            Name = input.Name,
            StartDate = input.StartDate.ToUniversalTime(),
            EndDate = input.EndDate.ToUniversalTime(),
            TeamId = input.TeamId
        };

        context.Sprints.Add(sprint);
        await context.SaveChangesAsync();

        return sprint;
    }

    [Authorize]
    public async Task<bool> DeleteSprint(
        Guid sprintId,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var sprint = await context.Sprints.Include(s => s.Team).FirstOrDefaultAsync(s => s.Id == sprintId);
        if (sprint == null) throw new GraphQLException("Sprint not found.");

        var userId = GetUserId(httpContextAccessor, context);
        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && sprint.Team.AdminId != userId)
        {
            throw new GraphQLException("Only team admins can delete sprints.");
        }

        context.Sprints.Remove(sprint);
        await context.SaveChangesAsync();

        return true;
    }

    [Authorize]
    public async Task<TeamInvitation> CreateTeamInvitation(
        CreateTeamInvitationDto input,
        [Service] ITeamInvitationService invitationService,
        [Service] ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserId(httpContextAccessor, context);

        var team = await context.Teams.FindAsync(input.TeamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);
        if (!isSuperAdmin && team.AdminId != userId)
        {
            throw new GraphQLException("Only team admins can invite users.");
        }

        try
        {
            var dto = await invitationService.CreateTeamInvitationAsync(input.TeamId, userId, input);
            return await context.TeamInvitations.FirstAsync(i => i.Id == dto.Id);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    [Authorize]
    public async Task<TeamInvitation> AcceptTeamInvitation(
        string token,
        [Service] ITeamInvitationService invitationService,
        [Service] ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserId(httpContextAccessor, context);
        try
        {
            var dto = await invitationService.AcceptTeamInvitationAsync(token, userId);
            // Retrieve the invitation even if it might be marked as accepted/deleted depending on logic
            return await context.TeamInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == dto.Id)
                   ?? throw new GraphQLException("Invitation processed but not found.");
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    [Authorize]
    public async Task<bool> DeleteTeamInvitation(
        Guid invitationId,
        [Service] ITeamInvitationService invitationService,
        [Service] ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserId(httpContextAccessor, context);

        var invitation = await context.TeamInvitations.FindAsync(invitationId);
        if (invitation == null) throw new GraphQLException("Invitation not found.");

        var team = await context.Teams.FindAsync(invitation.TeamId);
        if (team == null) throw new GraphQLException("Team not found.");

        var isSuperAdmin = IsSuperAdmin(httpContextAccessor);

        if (!isSuperAdmin && team.AdminId != userId)
        {
            throw new GraphQLException("Only team admins can delete invitations.");
        }

        try
        {
            await invitationService.DeleteTeamInvitationAsync(invitationId, userId);
            return true;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }
}