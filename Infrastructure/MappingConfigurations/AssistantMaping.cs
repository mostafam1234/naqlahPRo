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
    public class AssistantMaping : IEntityTypeConfiguration<Assistant>
    {

        public void Configure(EntityTypeBuilder<Assistant> builder)
        {
            builder.ToTable("NA_Assistant");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.DeliveryMan)
                .WithMany(x=>x.Assistants)
                .HasForeignKey(x => x.DeliveryManId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x=>x.AssistanWork)
                .WithMany()
                .HasForeignKey(x=>x.AssistanWorkId);
        }
    }
}
