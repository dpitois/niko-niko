using NikoNiko.Core.DTOs.User;
using NikoNiko.Core.DTOs.User.Export;
using NikoNiko.Core.Models;

namespace NikoNiko.Core.Interfaces;

public interface IUserService
{
    Task<UserExportDto?> GetExportDataAsync(Guid userId);
    Task<IEnumerable<UserDto>> GetUsersAsync(Guid userId, bool isSuperAdmin);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task DeleteUserAsync(Guid userId, Guid authenticatedUserId, bool isSuperAdmin);
    Task<User?> SubmitConsentAsync(Guid userId, UserConsentDto consentDto);
}