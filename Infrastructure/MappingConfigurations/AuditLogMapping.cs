using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class AuditLogMapping : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ActionName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.TimestampUtc)
                .IsRequired();

            builder.Property(a => a.IpAddress)
                .HasMaxLength(64);

            builder.Property(a => a.UserAgent)
                .HasMaxLength(512);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            builder.HasMany(a => a.Details)
                .WithOne(d => d.AuditLog)
                .HasForeignKey(d => d.AuditLogId);
        }
    }
}

