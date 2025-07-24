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
    public class EstablishMentMaping : IEntityTypeConfiguration<EstablishMent>
    {
        public void Configure(EntityTypeBuilder<EstablishMent> builder)
        {
            builder.ToTable("NA_EstablishMent");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.EstablishMentRepresentitive)
                   .WithOne(x => x.EstablishMent)
                   .HasForeignKey<EstablishMentRepresentitive>(x => x.EstablishmentId)
                   .IsRequired();
        }
    }
}
