using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("NA_User");

            builder.HasOne(x => x.DeliveryMan)
                .WithOne(x => x.User)
                .HasForeignKey<DeliveryMan>(x => x.UserId)
                .IsRequired();
        }
    }
}
