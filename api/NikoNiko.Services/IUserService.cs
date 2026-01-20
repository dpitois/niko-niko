using NikoNiko.Core.DTOs.User.Export;

namespace NikoNiko.Services;

public interface IUserService
{
    Task<UserExportDto?> GetExportDataAsync(Guid userId);
}
