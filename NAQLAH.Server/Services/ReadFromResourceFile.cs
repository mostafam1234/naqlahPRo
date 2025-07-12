using Domain.InterFaces;

namespace NAQLAH.Server.Services
{
    public class ReadFromResourceFile: IReadFromResourceFile
    {
        private readonly LockServices lockServices;

        public ReadFromResourceFile(LockServices lockServices)
        {
            this.lockServices = lockServices;
        }
        public string GetLocalizedMessage(string key)
        {
            var localizedString = lockServices.ReadFromResourceFile(key);
            return localizedString.Value;
        }
    }
}
