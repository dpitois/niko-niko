using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NikoNiko.Data; // Updated using directive
using NikoNiko.Core.DTOs.Badge; // Updated using directive

namespace NikoNiko.Api.Controllers;

/// <summary>
/// Controller for retrieving badges.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BadgesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BadgesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a list of all badges.
    /// </summary>
    /// <returns>A list of BadgeDto objects.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetBadges()
    {
        var badges = await _context.Badges
            .Select(b => new BadgeDto
            {
                Id = b.Id,
                UserId = b.UserId,
                Name = b.Name,
                Description = b.Description,
                EarnedAt = b.EarnedAt
            })
            .ToListAsync();

        return Ok(badges);
    }
}