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
    public class CategorySizeMaping : IEntityTypeConfiguration<Domain.Models.CategorySize>
    {
        public void Configure(EntityTypeBuilder<CategorySize> builder)
        {
            builder.ToTable("NA_CategorySize");
            builder.HasKey(x => x.Id);
        }
    }
}
