using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NikoNiko.Data;

namespace NikoNiko.Data.Sqlite;

public static class SQLiteDbContextOptionsExtensions
{
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("SQLiteConnection")));
        return services;
    }
}