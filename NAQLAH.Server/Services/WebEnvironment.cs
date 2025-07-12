using Domain.InterFaces;

namespace NAQLAH.Server.Services
{
    public class WebEnvironment : IWebEnvironment
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public WebEnvironment(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        public string WebRootPath
        {
            get
            {
                return webHostEnvironment.WebRootPath;
            }
        }
    }
}
