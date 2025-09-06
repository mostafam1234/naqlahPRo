using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace NAQLAH.Server.ApiDependencyInjection
{
    public static class FireBaseConfigurations
    {
        public static IServiceCollection AddFireBaseConfigurations(this IServiceCollection services, IConfiguration configuraion, IWebHostEnvironment webEnvironment)
        {
            var path = Path.Combine(webEnvironment.ContentRootPath, "FireBaseConfigurations.json");
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)
            });


            return services;
        }
    }
}
