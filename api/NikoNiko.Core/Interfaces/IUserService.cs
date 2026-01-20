using NikoNiko.Core.DTOs.User.Export;

namespace NikoNiko.Core.Interfaces;

public interface IUserService
{
    Task<UserExportDto?> GetExportDataAsync(Guid userId);
}
