namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data needed to create a new team.
/// </summary>
public record CreateTeamDto
{
    /// <summary>
    /// The name of the team.
    /// </summary>
    public string Name { get; init; } = null!;
}