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
    public class WalletTransctionMaping : IEntityTypeConfiguration<WalletTransctions>
    {
        public void Configure(EntityTypeBuilder<WalletTransctions> builder)
        {
            builder.ToTable("NA_WalletTransctions");
            builder.HasKey(x => x.Id);
        }
    }
}
