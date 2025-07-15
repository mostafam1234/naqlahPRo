using Domain.InterFaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class MediaUploader : IMediaUploader
    {
        private readonly IWebEnvironment _webEnvironment;

        public MediaUploader(IWebEnvironment webEnvironment)
        {
            this._webEnvironment = webEnvironment;
        }


        public async Task<string> UploadFromBase64(string imageBase64,string folderPath)
        {
            try
            {
                var data = imageBase64.Split(",").ToList().Last();

                byte[] imageBytes = Convert.FromBase64String(data);
                var extension = FileExtensionExtractor.GetExtension(imageBytes);
                string shortId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string fileName = string.Format("{0}.{1}", shortId, extension);
                string uploadsFolder = Path.Combine(_webEnvironment.WebRootPath, "ImageBank",folderPath);
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Ensure the uploads folder exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Write the image bytes to a file
                await File.WriteAllBytesAsync(filePath, imageBytes);

                return fileName;
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
            
        }
        public async Task<string> SaveFile(IFormFile file, string? oldFilePath = null)
        {
            if (!string.IsNullOrEmpty(oldFilePath))
            {
                var fullOldFilePath = Path.Combine(_webEnvironment.WebRootPath, oldFilePath);
                if (System.IO.File.Exists(fullOldFilePath))
                {
                    System.IO.File.Delete(fullOldFilePath);
                }
            }

            string uploadFolder = Path.Combine(_webEnvironment.WebRootPath, "ImageBank");
            Directory.CreateDirectory(uploadFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileSteam = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileSteam);
            }

            return uniqueFileName;
        }
    }
}
