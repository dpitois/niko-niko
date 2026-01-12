using System.ComponentModel.DataAnnotations;

using NikoNiko.Core.Models;

namespace NikoNiko.Core.DTOs.Mood;

/// <summary>
/// Represents the data needed to create a new mood entry.
/// </summary>
public class CreateMoodEntryDto
{
    /// <summary>
    /// The ID of the user making the entry.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// The ID of the sprint for this entry.
    /// </summary>
    [Required]
    public Guid SprintId { get; set; }

    /// <summary>
    /// The mood type (Happy, Neutral, Sad).
    /// </summary>
    [Required]
    public MoodType Mood { get; set; }

    /// <summary>
    /// The date of the mood entry. Defaults to the current date if not provided.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// The client's timezone offset in minutes from UTC.
    /// Positive values are East of UTC, negative values are West of UTC (e.g., +60 for UTC+1).
    /// </summary>
    public int TimezoneOffset { get; set; }
}