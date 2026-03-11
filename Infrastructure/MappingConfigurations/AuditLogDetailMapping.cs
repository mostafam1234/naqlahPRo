using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class AuditLogDetailMapping : IEntityTypeConfiguration<AuditLogDetail>
    {
        public void Configure(EntityTypeBuilder<AuditLogDetail> builder)
        {
            builder.ToTable("AuditLogDetails");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.EntityType)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(d => d.EntityId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(d => d.ChangeType)
                .IsRequired();

            builder.Property(d => d.OldValuesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(d => d.NewValuesJson)
                .HasColumnType("nvarchar(max)");
        }
    }
}

