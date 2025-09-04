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
    public class VehicleTypeCategoryMaping : IEntityTypeConfiguration<VehiclTypeCategory>
    {
        public void Configure(EntityTypeBuilder<VehiclTypeCategory> builder)
        {
            builder.ToTable("NA_VehicleTypeCategory");
            builder.HasKey(x => new {x.VehicleTypeId,x.MainCategoryId});
            builder.HasOne(x => x.VehicleType)
                   .WithMany(x=>x.VehicleTypeCategoies)
                   .HasForeignKey(x => x.VehicleTypeId);

            builder.HasOne(x => x.MainCategory).WithMany().HasForeignKey(x => x.MainCategoryId);
        }
    }
}
