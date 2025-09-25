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
    public class OrderServiceMaping : IEntityTypeConfiguration<OrderService>
    {
        public void Configure(EntityTypeBuilder<OrderService> builder)
        {
            builder.ToTable("NA_OrderServices");
            builder.HasKey(x => x.Id);
            builder.HasOne(x=>x.AssistanWork).WithMany().HasForeignKey(x=>x.WorkId);

        }
    }
}
