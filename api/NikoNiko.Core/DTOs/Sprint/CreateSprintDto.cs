using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.DTOs.Sprint;

/// <summary>
/// Represents the data needed to create a new sprint.
/// </summary>
public class CreateSprintDto
{
    /// <summary>
    /// The name of the sprint.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The start date of the sprint.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the sprint.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The ID of the team this sprint belongs to.
    /// </summary>
    [Required]
    public Guid TeamId { get; set; }
}