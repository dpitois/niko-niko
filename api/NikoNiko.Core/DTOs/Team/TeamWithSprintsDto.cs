using NikoNiko.Core.DTOs.Sprint;

namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data of a team with its associated sprints for display.
/// </summary>
public class TeamWithSprintsDto : TeamDto
{
    /// <summary>
    /// The list of sprints associated with the team.
    /// </summary>
    public List<SprintDto> Sprints { get; set; } = new();
}