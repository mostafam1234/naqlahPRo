using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class CategorySize
    {
        public CategorySize()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
        }
        public int Id { get; set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }
        public double MinimumAmout { get;private set; }
        public double MaximumAmount { get;private set; }
        public CategorySizeUnitMeasurment CategorySizeUnit { get;private set; }

    }
}
