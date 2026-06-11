using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class CustomerNotificationMapping : IEntityTypeConfiguration<CustomerNotification>
    {
        public void Configure(EntityTypeBuilder<CustomerNotification> builder)
        {
            builder.ToTable("NA_CustomerNotification");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.NotificationId).IsRequired();
            builder.Property(x => x.CustomerId).IsRequired();
            builder.Property(x => x.IsRead).IsRequired();
            builder.Property(x => x.ReadDate).IsRequired(false);
            builder.Property(x => x.CreatedDate).IsRequired();

            builder.HasOne(x => x.Notification)
                .WithMany(x => x.CustomerNotifications)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => new { x.CustomerId, x.IsRead });
        }
    }
}
