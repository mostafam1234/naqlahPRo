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
    public class OrderWayPointMaping : IEntityTypeConfiguration<OrderWayPoint>
    {
        public void Configure(EntityTypeBuilder<OrderWayPoint> builder)
        {
            builder.ToTable("NA_OrderWayPoint");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Neighborhood)
                   .WithMany()
                   .HasForeignKey(x => x.NeighborhoodId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x=>x.City)
                   .WithMany()
                   .HasForeignKey(x=>x.CityId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x=>x.Region)
                   .WithMany()
                   .HasForeignKey(x=>x.RegionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
