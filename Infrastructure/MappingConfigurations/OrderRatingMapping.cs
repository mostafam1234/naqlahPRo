using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class OrderRatingMapping : IEntityTypeConfiguration<OrderRating>
    {
        public void Configure(EntityTypeBuilder<OrderRating> builder)
        {
            builder.ToTable("NA_OrderRating");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Rating).IsRequired();
            builder.Property(x => x.Comment).HasMaxLength(1000);
            builder.Property(x => x.CreatedDate).IsRequired();

            builder.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DeliveryMan>().WithMany().HasForeignKey(x => x.DeliveryManId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OrderId, x.CustomerId }).IsUnique();
        }
    }
}
