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
    public class OrderPaymentMethodMaping : IEntityTypeConfiguration<OrderPaymentMethod>
    {
        public void Configure(EntityTypeBuilder<OrderPaymentMethod> builder)
        {
            builder.ToTable("NA_OrderPaymentMethod");
            builder.HasKey(x => new { x.OrderId, x.PaymentMethodId });
            builder.HasOne(x => x.Order)
                   .WithMany(x => x.PaymentMethods)
                   .HasForeignKey(x => x.OrderId);

            builder.HasOne(x => x.PaymentMethod).WithMany()
                   .HasForeignKey(x => x.PaymentMethodId);
        }
    }
}
