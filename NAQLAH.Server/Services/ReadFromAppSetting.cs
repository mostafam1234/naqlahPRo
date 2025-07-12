using Domain.InterFaces;

namespace NAQLAH.Server.Services
{
    public class ReadFromAppSetting: IReadFromAppSetting
    {
        private readonly IConfiguration configuration;

        public ReadFromAppSetting(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public T GetValue<T>(string key)
        {
            return configuration.GetValue<T>(key);
        }
    }
}
