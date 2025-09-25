using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class VehiclTypeCategory
    {
        private VehiclTypeCategory()
        {
            
        }
        public int VehicleTypeId { get; set; }
        public int MainCategoryId { get; set; }
        public VehicleType VehicleType { get; set; }
        public MainCategory MainCategory { get; set; }
    }
}
