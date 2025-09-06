using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class VehicleType
    {
        public VehicleType()
        {
            this.ArabicName = string.Empty;
            this.EnglishName = string.Empty;
            this.VehicleTypeCategoies = new List<VehiclTypeCategory>();
        }
        public int Id { get;private set; }
        public string ArabicName { get;private set; }
        public string EnglishName { get;private set; }
        public string IconImagePath { get;private set; }
        private List<VehiclTypeCategory> _VehicleTypeCategoies { get; set; }
        public IReadOnlyList<VehiclTypeCategory> VehicleTypeCategoies
        {
            get
            {
                return _VehicleTypeCategoies;
            }
            private set
            {
                _VehicleTypeCategoies = (List<VehiclTypeCategory>)value.ToList();
            }
        }
    }
}
