using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Data;

namespace NikoNiko.Api.Authorization;

public class IsTeamMemberHandler : AuthorizationHandler<IsTeamMemberRequirement>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IsTeamMemberHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsTeamMemberRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return;
        }

        if (context.User.HasClaim("is_super_admin", "true"))
        {
            context.Succeed(requirement);
            return;
        }

        Guid teamId = Guid.Empty;

        // Try to get teamId from route values
        object? teamIdRouteValue = httpContext.GetRouteValue("teamId");
        if (teamIdRouteValue != null && Guid.TryParse(teamIdRouteValue.ToString(), out teamId))
        {
            // teamId found in route
        }
        else
        {
            // If teamId not in route, try to get sprintId and then find teamId
            object? sprintIdRouteValue = httpContext.GetRouteValue("sprintId");
            if (sprintIdRouteValue != null && Guid.TryParse(sprintIdRouteValue.ToString(), out var sprintId))
            {
                var sprint = await _dbContext.Sprints.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId);
                if (sprint != null)
                {
                    teamId = sprint.TeamId;
                }
            }
        }

        if (teamId == Guid.Empty)
        {
            // No valid teamId could be determined
            return;
        }

        var isMember = await _dbContext.TeamUsers
            .AnyAsync(tu => tu.TeamId == teamId && tu.UserId == userId);

        var team = await _dbContext.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId); // Use AsNoTracking for read-only query
        var isTeamAdmin = team?.AdminId == userId;

        if (isMember || isTeamAdmin)
        {
            context.Succeed(requirement);
        }
    }
}