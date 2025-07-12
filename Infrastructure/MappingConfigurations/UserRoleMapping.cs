using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class UserRoleMapping : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("NA_UserRoles");
            builder.HasKey(x => new { x.RoleId, x.UserId });

            builder.HasOne<Role>()
                .WithMany(x => x.AspNetUserRoles)
                .HasForeignKey(x => x.RoleId);

            builder.HasOne<User>()
                .WithMany(x => x.AspNetUserRoles)
                .HasForeignKey(x => x.UserId);

        }
    }
}
