using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecMan.Data.Audit;
using SecMan.Interfaces.DAL;

namespace SecMan.Data.Config
{
    public static class Config
    {
        public static IServiceCollection ConfigureDALServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SQLCipher.Db>((serviceProvider, options) =>
            {
                var databaseFile = "SecMan.db";
                var DbPath = System.IO.Path.Join(configuration["DBPath"], databaseFile);

                options.UseSqlite($"Data Source={DbPath}").EnableSensitiveDataLogging().EnableDetailedErrors();
            });

            services.AddScoped<IAuditServices, AuditServices>();


            return services;
        }
    }
}
