using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class CaptainNotificationMapping : IEntityTypeConfiguration<CaptainNotification>
    {
        public void Configure(EntityTypeBuilder<CaptainNotification> builder)
        {
            builder.ToTable("NA_CaptainNotifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Body).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.MissingFieldsJson).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.LegalDisclaimer).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
            builder.HasOne(x => x.DeliveryMan)
                .WithMany()
                .HasForeignKey(x => x.DeliveryManId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.DeliveryManId);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
