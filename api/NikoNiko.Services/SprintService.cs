using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;

namespace NikoNiko.Services;

public class SprintService : ISprintService
{
    private readonly ApplicationDbContext _context;

    public SprintService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SprintDto>> GetSprintsAsync(Guid? teamId, Guid userId, bool isSuperAdmin)
    {
        throw new NotImplementedException();
    }

    public async Task<SprintDto?> GetSprintByIdAsync(Guid sprintId)
    {
        throw new NotImplementedException();
    }

    public async Task<SprintDto> CreateSprintAsync(CreateSprintDto createSprintDto, Guid userId, bool isSuperAdmin)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateSprintAsync(Guid sprintId, UpdateSprintDto updateSprintDto)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteSprintAsync(Guid sprintId)
    {
        throw new NotImplementedException();
    }
}
