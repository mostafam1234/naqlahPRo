using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class AdditionalServiceMapping : IEntityTypeConfiguration<AdditionalService>
    {
        public void Configure(EntityTypeBuilder<AdditionalService> builder)
        {
            builder.ToTable("NA_AdditionalService");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ArabicName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.EnglishName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        }
    }
}
