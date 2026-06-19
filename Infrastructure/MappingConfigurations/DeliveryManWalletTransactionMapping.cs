using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class DeliveryManWalletTransactionMapping : IEntityTypeConfiguration<DeliveryManWalletTransaction>
    {
        public void Configure(EntityTypeBuilder<DeliveryManWalletTransaction> builder)
        {
            builder.ToTable("NA_DeliveryManWalletTransaction");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.DescriptionAr).HasMaxLength(1000);
            builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
            builder.HasOne(x => x.DeliveryMan)
                .WithMany()
                .HasForeignKey(x => x.DeliveryManId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.DeliveryManId);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
