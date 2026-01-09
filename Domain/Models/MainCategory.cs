using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MainCategory
    {
        private MainCategory()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this.ImagePath = string.Empty;
        }
        public int Id { get; private set; }
        public string ArabicName { get; private set; }
        public string EnglishName { get; private set; }
        public string ImagePath { get; private set; }

        public static Result<MainCategory> Instance(string arabicName, string englishName, string imagePath)
        {
            return new MainCategory()
            {
                ArabicName = arabicName,
                EnglishName = englishName,
                ImagePath = imagePath
            };
        }

        public void Update(string arabicName, string englishName, string imagePath)
        {
            ArabicName = arabicName;
            EnglishName = englishName;
            ImagePath = imagePath;
        }

    }
}
