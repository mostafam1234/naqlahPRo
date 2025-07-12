using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class RoleClaimMapping : IEntityTypeConfiguration<RoleClaims>
    {
        public void Configure(EntityTypeBuilder<RoleClaims> builder)
        {
            builder.ToTable("NA_RoleClaims");
        }
    }
}
