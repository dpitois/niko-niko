namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data of a team for display.
/// </summary>
public class TeamDto
{
    /// <summary>
    /// The unique identifier of the team.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The name of the team.
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// The ID of the team's administrator.
    /// </summary>
    public Guid AdminId { get; set; }
    /// <summary>
    /// The name of the team's administrator.
    /// </summary>
    public string AdminName { get; set; } = null!;
    /// <summary>
    /// The date and time when the team was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}