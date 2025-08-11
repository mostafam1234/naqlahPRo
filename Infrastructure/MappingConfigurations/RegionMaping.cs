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
    public class RegionMaping : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable("NA_Region");
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Cities)
                   .WithOne()
                   .HasForeignKey(x => x.RegionId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
