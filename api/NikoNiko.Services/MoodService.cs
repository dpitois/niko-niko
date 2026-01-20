using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class MoodService : IMoodService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public MoodService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<MoodEntryDto>> GetMoodEntriesAsync(Guid userId, bool isSuperAdmin)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<MoodEntryDto>> GetMyMoodEntriesAsync(Guid userId, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<MoodEntryDto?> GetMoodEntryByIdAsync(Guid moodEntryId)
    {
        throw new NotImplementedException();
    }

    public async Task<MoodEntryDto> CreateOrUpdateMoodEntryAsync(CreateMoodEntryDto createMoodEntryDto, Guid authenticatedUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<MoodEntryDto>> GetMoodEntriesBySprintAsync(Guid sprintId, Guid? userId, DateTime? date, Guid authenticatedUserId, bool isSuperAdmin)
    {
        throw new NotImplementedException();
    }
}
