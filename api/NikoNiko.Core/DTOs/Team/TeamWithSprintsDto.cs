using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.User; // New using directive

namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data of a team with its associated sprints and members for display.
/// </summary>
public class TeamWithSprintsDto : TeamDto
{
    /// <summary>
    /// The list of sprints associated with the team.
    /// </summary>
    public List<SprintDto> Sprints { get; set; } = new();

    /// <summary>
    /// The list of users who are members of the team.
    /// </summary>
    public List<UserDto> Members { get; set; } = new();
}