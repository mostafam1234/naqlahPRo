using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class UserTokenMapping : IEntityTypeConfiguration<UserTokens>
    {
        public void Configure(EntityTypeBuilder<UserTokens> builder)
        {
            builder.ToTable("NA_UserTokens");
        }
    }
}
