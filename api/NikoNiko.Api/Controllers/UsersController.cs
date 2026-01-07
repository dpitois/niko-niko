using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs.User;
using NikoNiko.Data;

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a list of all users.
    /// Super-admins get all users. Regular users get users from teams they are a member of.
    /// </summary>
    /// <returns>A list of UserDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        IQueryable<Core.Models.User> query;

        if (User.HasClaim("is_super_admin", "true"))
        {
            query = _context.Users;
        }
        else
        {
            // Get teams the current user is in
            var userTeamIds = await _context.TeamUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TeamId)
                .ToListAsync();

            // Get all users who are in those teams
            query = _context.Users
                .Where(u => u.TeamUsers.Any(tu => userTeamIds.Contains(tu.TeamId)));
        }

        var users = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt
            })
            .Distinct()
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>The UserDto object, or NotFound if the user does not exist.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}