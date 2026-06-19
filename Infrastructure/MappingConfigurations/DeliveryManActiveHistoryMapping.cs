using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class DeliveryManActiveHistoryMapping : IEntityTypeConfiguration<DeliveryManActiveHistory>
    {
        public void Configure(EntityTypeBuilder<DeliveryManActiveHistory> builder)
        {
            builder.ToTable("NA_DeliveryManActiveHistory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Active).IsRequired();
            builder.Property(x => x.ChangedAt).IsRequired();

            builder.HasOne(x => x.DeliveryMan)
                .WithMany()
                .HasForeignKey(x => x.DeliveryManId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.DeliveryManId);
            builder.HasIndex(x => x.ChangedAt);
        }
    }
}
