using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.LogIn
{
    public class AdminResponse
    {
        public bool IsActive { get; set; }
        public TokenAdminResponse TokenResponse { get; set; }
    }
}
