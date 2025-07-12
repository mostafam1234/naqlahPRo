using Domain.InterFaces;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HangFireDepencies
{
    public static class HangFireConfigurations
    {
        public static IServiceCollection AddHangFireConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x =>
            {
                x.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                         .UseSimpleAssemblyNameTypeSerializer()
                         .UseRecommendedSerializerSettings()
                         .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<HangfireBridge>();
            services.AddScoped<IHangfireJobWriter, HangfireJobWriter>();

            return services;
        }
    }
}
