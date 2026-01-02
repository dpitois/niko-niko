using NikoNiko.Core.Models;

namespace NikoNiko.Core.DTOs.Mood;

/// <summary>
/// Represents the data of a mood entry for display.
/// </summary>
public class MoodEntryDto
{
    /// <summary>
    /// The unique identifier of the mood entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the user who made the entry.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The ID of the sprint for this entry.
    /// </summary>
    public Guid SprintId { get; set; }

    /// <summary>
    /// The date of the mood entry.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The mood type (Happy, Neutral, Sad).
    /// </summary>
    public MoodType Mood { get; set; }
}