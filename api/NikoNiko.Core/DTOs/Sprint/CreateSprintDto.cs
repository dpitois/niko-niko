using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.DTOs.Sprint;

/// <summary>
/// Represents the data needed to create a new sprint.
/// </summary>
public record CreateSprintDto
{
    /// <summary>
    /// The name of the sprint.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The start date of the sprint.
    /// </summary>
    [Required]
    public DateOnly StartDate { get; init; }

    /// <summary>
    /// The end date of the sprint.
    /// </summary>
    [Required]
    public DateOnly EndDate { get; init; }

    /// <summary>
    /// The ID of the team this sprint belongs to.
    /// </summary>
    [Required]
    public Guid TeamId { get; init; }
}