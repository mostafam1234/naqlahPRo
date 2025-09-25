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
    public class OrderMaping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("NA_Order");
            builder.HasKey(x => x.Id);
            builder.HasMany(x=>x.OrderDetails).WithOne().HasForeignKey(x=>x.OrderId);
            builder.HasMany(x => x.OrderWayPoints).WithOne().HasForeignKey(x => x.OrderId);
            builder.HasMany(x => x.PaymentMethods).WithOne().HasForeignKey(x => x.OrderId);
            builder.HasMany(x => x.OrderStatusHistories).WithOne().HasForeignKey(x => x.OrderId);
            builder.HasOne(x=>x.OrderPackage).WithMany().HasForeignKey(x=>x.OrderPackageId);
            builder.HasMany(x=>x.OrderServices).WithOne().HasForeignKey(x=>x.OrderId);
        }
    }
}
