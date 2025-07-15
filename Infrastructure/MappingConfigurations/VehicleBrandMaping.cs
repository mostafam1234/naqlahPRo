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
    public class VehicleBrandMaping : IEntityTypeConfiguration<VehicleBrand>
    {
        public void Configure(EntityTypeBuilder<VehicleBrand> builder)
        {
            builder.ToTable("NA_VehicleBrand");
            builder.HasKey(x => x.Id);
        }
    }
}
