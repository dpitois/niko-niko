namespace NikoNiko.Core.DTOs.Sprint;

/// <summary>
/// Represents the data of a sprint for display.
/// </summary>
public class SprintDto
{
    /// <summary>
    /// The unique identifier of the sprint.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the sprint.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The start date of the sprint.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the sprint.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The ID of the team this sprint belongs to.
    /// </summary>
    public Guid TeamId { get; set; }
}