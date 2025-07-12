using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class UserLoginMapping : IEntityTypeConfiguration<UserLogin>
    {
        public void Configure(EntityTypeBuilder<UserLogin> builder)
        {
            builder.ToTable("NA_UserLogins");
            builder.HasOne<User>()
                .WithMany(x => x.AspNetUserLogins)
                .HasForeignKey(x => x.UserId);

        }
    }
}
