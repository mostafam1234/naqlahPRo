using Domain.InterFaces;
using Infrastructure.Services;
using NAQLAH.Server.Services;

namespace NAQLAH.Server.ApiDependencyInjection
{
    public static class ApiDependencyInjection
    {
        public static IServiceCollection AddServicesForApi(this IServiceCollection services)
        {
            services.AddScoped<UserSession>();
            services.AddScoped<IUserSession, UserSessions>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<LockServices>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IWebEnvironment, WebEnvironment>();
            services.AddScoped<IMediaUploader, MediaUploader>();
            services.AddSingleton<IReadFromResourceFile, ReadFromResourceFile>();
            services.AddHttpClient();
            services.AddSingleton<IReadFromAppSetting, ReadFromAppSetting>();
            return services;
        }
    }
}
