namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// DTO for updating team details.
/// </summary>
public record UpdateTeamDto
{
    /// <summary>
    /// The new name of the team.
    /// </summary>
    public required string Name { get; init; }
}
