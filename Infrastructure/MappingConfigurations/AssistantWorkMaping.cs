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
    public class AssistantWorkMaping : IEntityTypeConfiguration<AssistanWork>
    {
        public void Configure(EntityTypeBuilder<AssistanWork> builder)
        {
            builder.ToTable("NA_AssistanWork");
            builder.HasKey(x => x.Id);
        }
    }
}
