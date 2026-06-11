using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class OrderAdditionalServiceMapping : IEntityTypeConfiguration<OrderAdditionalService>
    {
        public void Configure(EntityTypeBuilder<OrderAdditionalService> builder)
        {
            builder.ToTable("NA_OrderAdditionalServices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.HasOne(x => x.AdditionalService).WithMany().HasForeignKey(x => x.AdditionalServiceId);
        }
    }
}
