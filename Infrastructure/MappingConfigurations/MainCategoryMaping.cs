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
    public class MainCategoryMaping : IEntityTypeConfiguration<MainCategory>
    {
        public void Configure(EntityTypeBuilder<MainCategory> builder)
        {
            builder.ToTable("NA_MainCategory");
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.CategorySizes)
                   .WithOne()
                   .HasForeignKey("MainCategoryId")
                   .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
