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
    public class OrderDetailsMaping : IEntityTypeConfiguration<OrderDetails>
    {
        public void Configure(EntityTypeBuilder<OrderDetails> builder)
        {
            builder.ToTable("NA_OrderDetails");
            builder.HasKey(x => x.Id);
            builder.HasOne(x=>x.MainCategory).WithMany().HasForeignKey(x=>x.MainCategoryId);

        }
    }
}
