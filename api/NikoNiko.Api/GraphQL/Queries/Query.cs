using System.Security.Claims;

using HotChocolate.Authorization;

using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Api.GraphQL.Queries;

public class Query
{
    public string Hello() => "World";

    [Authorize]
    public async Task<User?> GetMe(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        var email = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
                    ?? httpContextAccessor.HttpContext?.User.Identity?.Name;

        if (string.IsNullOrEmpty(email)) return null;

        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    [Authorize]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Team> GetMyTeams(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Note: OAuth setups might vary on what is in NameIdentifier vs Email.
        // Assuming we rely on Email for mapping for now as done in GetMe, or better, fetch User first.

        var email = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
                    ?? httpContextAccessor.HttpContext?.User.Identity?.Name;

        if (string.IsNullOrEmpty(email)) return Enumerable.Empty<Team>().AsQueryable();

        // Returning teams where the user is a member or admin
        return dbContext.Teams
            .Where(t => t.TeamUsers.Any(tu => tu.User.Email == email) || t.Admin.Email == email)
            .AsNoTracking();
    }

    [Authorize]
    public async Task<Team?> GetTeam(Guid id, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        var email = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
                   ?? httpContextAccessor.HttpContext?.User.Identity?.Name;

        if (string.IsNullOrEmpty(email)) return null;

        var team = await dbContext.Teams
            .Include(t => t.Sprints)
            .Include(t => t.TeamUsers)
            .ThenInclude(tu => tu.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return null;

        // Check access: User must be member or admin
        var isMember = team.TeamUsers.Any(tu => tu.User.Email == email);
        var isAdmin = team.Admin?.Email == email; // Admin property might not be loaded if not included, but we can check AdminId if we loaded user separately or check relationship.

        // Optimisation: check AdminId via subquery or if loaded. 
        // For now, let's assume we need to verify access.
        // A better approach is to filter in the DB query directly.

        var hasAccess = await dbContext.Teams.AnyAsync(t => t.Id == id && (
            t.Admin.Email == email || t.TeamUsers.Any(tu => tu.User.Email == email)
        ));

        return hasAccess ? team : null;
    }
}