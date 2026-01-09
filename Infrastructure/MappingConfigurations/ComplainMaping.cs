using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MappingConfigurations
{
    public class ComplainMaping : IEntityTypeConfiguration<Complain>
    {
        public void Configure(EntityTypeBuilder<Complain> builder)
        {
            builder.ToTable("NA_Complains");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.CustomerId).IsRequired();
            builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(500);
            builder.Property(x => x.CustomerMobileNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.CustomerAddress).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.CreationDate).IsRequired();
        }
    }
}

