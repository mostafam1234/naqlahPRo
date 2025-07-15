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
    public class VehicleTypeMaping : IEntityTypeConfiguration<VehicleType>
    {
        public void Configure(EntityTypeBuilder<VehicleType> builder)
        {
            builder.ToTable("NA_VehicleType");
            builder.HasKey(x => x.Id);
        }
    }
}
