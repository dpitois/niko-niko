using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.DTOs.User; // New using directive

namespace NikoNiko.Core.DTOs.Team;

/// <summary>
/// Represents the data of a team with its associated sprints and members for display.
/// </summary>
public record TeamWithSprintsDto : TeamDto
{
    /// <summary>
    /// The list of sprints associated with the team.
    /// </summary>
    public List<SprintDto> Sprints { get; init; } = new();

    /// <summary>
    /// The list of users who are members of the team.
    /// </summary>
    public List<UserDto> Members { get; init; } = new();
}