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
    public class IndividualMaping : IEntityTypeConfiguration<Individual>
    {
        public void Configure(EntityTypeBuilder<Individual> builder)
        {
            builder.ToTable("NA_Individual");
            builder.HasKey(x => x.Id);
        }
    }
}
