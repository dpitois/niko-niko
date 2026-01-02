using System.ComponentModel.DataAnnotations;

namespace NikoNiko.Core.Models;

public class Sprint
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public List<MoodEntry> MoodEntries { get; set; } = new();
}