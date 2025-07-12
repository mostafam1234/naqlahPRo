using Domain.InterFaces;
using NAQLAH.Server.Services;

namespace NAQLAH.Server.ApiDependencyInjection
{
    public static class ApiDependencyInjection
    {
        public static IServiceCollection AddServicesForApi(this IServiceCollection services)
        {
            services.AddScoped<UserSession>();
            services.AddScoped<IUserSession, UserSessions>();
            services.AddSingleton<LockServices>();
            services.AddSingleton<IReadFromResourceFile, ReadFromResourceFile>();
            services.AddHttpClient();
            services.AddSingleton<IReadFromAppSetting, ReadFromAppSetting>();
            return services;
        }
    }
}
