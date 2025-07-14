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
    public class DeliveryManMaping : IEntityTypeConfiguration<DeliveryMan>
    {
        public void Configure(EntityTypeBuilder<DeliveryMan> builder)
        {
            builder.ToTable("NA_DeliveryMan");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Vehicle)
                   .WithOne(x => x.DeliveryMan)
                   .HasForeignKey<DeliveryVehicle>(x => x.DeliveryManId)
                   .IsRequired();

        }
    }
}
