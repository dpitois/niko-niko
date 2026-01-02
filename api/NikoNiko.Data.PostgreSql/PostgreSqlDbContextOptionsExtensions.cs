using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Data;

namespace NikoNiko.Data.PostgreSql;

public static class PostgreSqlDbContextOptionsExtensions
{
    public static IServiceCollection AddPostgreSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
