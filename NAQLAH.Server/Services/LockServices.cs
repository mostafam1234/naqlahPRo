using Microsoft.Extensions.Localization;
using NAQLAH.Server.Resources;
using System.Reflection;

namespace NAQLAH.Server.Services
{
    public class LockServices
    {
        private readonly IStringLocalizer _localizer;

        public LockServices(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo()?.Assembly?.FullName??"");
            _localizer = factory.Create("SharedResource", assemblyName?.Name??"");
        }

        public LocalizedString ReadFromResourceFile(string key)
        {
            return _localizer[key];
        }
    }

}
