using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class NotificationMapping : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("NA_Notifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicTitle).IsRequired().HasMaxLength(200);
            builder.Property(x => x.EnglishTitle).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ArabicMessage).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.EnglishMessage).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.OrderId).IsRequired(false);
            builder.Property(x => x.NotificationType).IsRequired();
            builder.Property(x => x.CreationDate).IsRequired();
            builder.Property(x => x.IsRead).IsRequired();
            builder.Property(x => x.UserId).IsRequired(false);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.IsRead);
            builder.HasIndex(x => x.CreationDate);
        }
    }
}

