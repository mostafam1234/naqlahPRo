using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class VehicleTypeMaping : IEntityTypeConfiguration<VehicleType>
    {
        public void Configure(EntityTypeBuilder<VehicleType> builder)
        {
            builder.ToTable("NA_VehicleType");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.EnglishName).IsRequired();
            builder.Property(x => x.IconImagePath).IsRequired();
            builder.Property(x => x.Cost).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ServiceFee).HasColumnType("decimal(18,2)");
            builder.Property(x => x.LoadCategory).HasConversion<int?>();
            builder.Property(x => x.CreationDate).IsRequired();
        }
    }
}
