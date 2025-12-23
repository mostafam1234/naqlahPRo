using Application.Services.GoogleMap;
using Domain.InterFaces;
using Infrastructure.Services;
using NAQLAH.Server.Services;
using TalabatkData.GoogleMapServices;

namespace NAQLAH.Server.ApiDependencyInjection
{
    public static class ApiDependencyInjection
    {
        public static IServiceCollection AddServicesForApi(this IServiceCollection services)
        {
            services.AddScoped<UserSession>();
            services.AddScoped<IUserSession, UserSessions>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IGoogleMapService, GoogleMapService>();
            services.AddSingleton<LockServices>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IWebEnvironment, WebEnvironment>();
            services.AddScoped<IMediaUploader, MediaUploader>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<IReadFromResourceFile, ReadFromResourceFile>();
            services.AddHttpClient();
            services.AddSingleton<IReadFromAppSetting, ReadFromAppSetting>();
            return services;
        }
    }
}
