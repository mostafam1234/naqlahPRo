using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface IMediaUploader
    {
        Task<string> UploadFromBase64(string imageBase64);
    }
}
