
using Domain.InterFaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DatabaseConfigurations
    {
        public static IServiceCollection AddDataBase(this IServiceCollection services, IConfiguration configuraion)
        {
            services.AddDbContext<NaqlahContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseSqlServer(configuraion.GetConnectionString("DefaultConnection"), x =>
                {

                })
                .EnableSensitiveDataLogging();
            });

            services.AddScoped<NaqlahContext>();
            services.AddScoped<INaqlahContext>(option => option.GetService<NaqlahContext>());

            return services;
        }
    }
}
