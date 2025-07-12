using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class UserClaimMapping : IEntityTypeConfiguration<UserClaim>
    {
        public void Configure(EntityTypeBuilder<UserClaim> builder)
        {
            builder.ToTable("NA_UserClaims");

            builder.HasOne<User>()
                .WithMany(x => x.AspNetUserClaims)
                .HasForeignKey(x => x.UserId);

        }
    }
}
