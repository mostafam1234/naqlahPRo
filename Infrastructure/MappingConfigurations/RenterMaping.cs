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
    public class RenterMaping : IEntityTypeConfiguration<Renter>
    {
        public void Configure(EntityTypeBuilder<Renter> builder)
        {
            builder.ToTable("NA_Renter");
        }
    }
}
