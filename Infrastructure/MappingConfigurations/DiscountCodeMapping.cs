using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class DiscountCodeMapping : IEntityTypeConfiguration<DiscountCode>
    {
        public void Configure(EntityTypeBuilder<DiscountCode> builder)
        {
            builder.ToTable("NA_DiscountCode");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.MinimumOrderAmount).HasPrecision(18, 2);
        }
    }
}
