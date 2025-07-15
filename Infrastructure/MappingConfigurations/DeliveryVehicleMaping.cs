using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MappingConfigurations
{
    public class DeliveryVehicleMaping : IEntityTypeConfiguration<DeliveryVehicle>
    {
        public void Configure(EntityTypeBuilder<DeliveryVehicle> builder)
        {
            builder.ToTable("NA_DeliveryVehicle");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId);
            builder.HasOne(x => x.VehicleBrand).WithMany().HasForeignKey(x => x.VehicleBrandId);
            builder.HasOne(x => x.Resident).WithOne().HasForeignKey<Resident>(x => x.DeliveryVehicleId);
            builder.HasOne(x => x.Company).WithOne().HasForeignKey<Company>(x => x.DeliveryVehicleId);
            builder.HasOne(x => x.Renter).WithOne().HasForeignKey<Renter>(x => x.DeliveryVehicleId);
        }
    }
}
