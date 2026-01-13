using NikoNiko.Core.Models;
using NikoNiko.Data;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Authorization;

namespace NikoNiko.Api.GraphQL.Queries;

public class Query
{
    public string Hello() => "World";

    public async Task<User?> GetMe(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        var email = httpContextAccessor.HttpContext?.User.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return null;
        
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
