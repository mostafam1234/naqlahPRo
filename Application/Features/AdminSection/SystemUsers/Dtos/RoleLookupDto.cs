using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.SystemUsers.Dtos
{
    public class RoleLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ArabicName { get; set; } = string.Empty;
    }
}

