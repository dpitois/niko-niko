using NikoNiko.Core.DTOs;
using NikoNiko.Core.DTOs.Mood;

namespace NikoNiko.Core.Interfaces;

public interface IMoodService
{
    Task<IEnumerable<MoodEntryDto>> GetMoodEntriesAsync(Guid userId, bool isSuperAdmin);
    Task<PagedResult<MoodEntryDto>> GetMyMoodEntriesAsync(Guid userId, int page, int pageSize);
    Task<MoodEntryDto?> GetMoodEntryByIdAsync(Guid moodEntryId);
    Task<MoodEntryDto> CreateOrUpdateMoodEntryAsync(CreateMoodEntryDto createMoodEntryDto, Guid authenticatedUserId);
    Task<IEnumerable<MoodEntryDto>> GetMoodEntriesBySprintAsync(Guid sprintId, Guid? userId, DateTime? date, Guid authenticatedUserId, bool isSuperAdmin);
}
