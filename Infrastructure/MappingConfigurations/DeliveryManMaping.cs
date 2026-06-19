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
            builder.Property(x => x.Address).IsRequired(false);
            builder.Property(x => x.BackIdenitytImagePath).IsRequired(false);
            builder.Property(x => x.BackDrivingLicenseImagePath).IsRequired(false);
            builder.Property(x => x.PersonalImagePath).IsRequired(false);
            builder.Property(x => x.BirthDate).IsRequired(false);
            builder.Property(x => x.IdentityExpirationDate).IsRequired(false);
            builder.Property(x => x.DrivingLicenseExpirationDate).IsRequired(false);
            builder.Property(x => x.MissingProfileFieldsJson).HasMaxLength(4000);
            builder.Property(x => x.RegisteredAt).IsRequired();
            builder.HasOne(x => x.Vehicle)
                   .WithOne(x => x.DeliveryMan)
                   .HasForeignKey<DeliveryVehicle>(x => x.DeliveryManId)
                   .IsRequired();
            builder.HasOne(x => x.DeliveryManLocation)
                   .WithOne()
                   .HasForeignKey<DeliveryManLocation>(x => x.DeliveryManId)
                   .IsRequired();

        }
    }
}
