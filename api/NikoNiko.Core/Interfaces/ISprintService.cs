using NikoNiko.Core.DTOs.Sprint;

namespace NikoNiko.Core.Interfaces;

public interface ISprintService
{
    Task<IEnumerable<SprintDto>> GetSprintsAsync(Guid? teamId, Guid userId, bool isSuperAdmin);
    Task<SprintDto?> GetSprintByIdAsync(Guid sprintId);
    Task<SprintDto> CreateSprintAsync(CreateSprintDto createSprintDto, Guid userId, bool isSuperAdmin);
    Task UpdateSprintAsync(Guid sprintId, UpdateSprintDto updateSprintDto);
    Task DeleteSprintAsync(Guid sprintId);
}
