using NikoNiko.Core.Models;

namespace NikoNiko.Services;

public interface ITokenService
{
    string CreateToken(User user);
}