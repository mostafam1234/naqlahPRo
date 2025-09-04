using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MappingConfigurations
{
    public class CustomerMaping : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("NA_Customer");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Individual)
                   .WithOne(x => x.Customer)
                   .HasForeignKey<Individual>(x => x.CustomerId)
                   .IsRequired();

            builder.HasOne(x => x.EstablishMent)
                   .WithOne(x => x.Customer)
                   .HasForeignKey<EstablishMent>(x => x.CustomerId)
                   .IsRequired();

            builder.HasMany(x=>x.WalletTransctions).WithOne().HasForeignKey(x=>x.CustomerId);

        }
    }
}
