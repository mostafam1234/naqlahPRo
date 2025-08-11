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
    public class CityMaping : IEntityTypeConfiguration<City>
    {
        
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("NA_City");
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Neighborhoods)
                   .WithOne()
                   .HasForeignKey(x => x.CityId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
