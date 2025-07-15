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
    public class ResidentMaping : IEntityTypeConfiguration<Resident>
    {
        public void Configure(EntityTypeBuilder<Resident> builder)
        {
            builder.ToTable("NA_Resident");
        }
    }
}
