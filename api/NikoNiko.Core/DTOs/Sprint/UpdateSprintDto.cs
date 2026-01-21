using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.DTOs.Sprint;

/// <summary>
/// Represents the data needed to update an existing sprint.
/// </summary>
public record UpdateSprintDto
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
    public DateTime StartDate { get; init; }

    /// <summary>
    /// The end date of the sprint.
    /// </summary>
    [Required]
    public DateTime EndDate { get; init; }
}