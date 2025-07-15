using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        public static UserRole Instance(int roleId)
        {
            return new UserRole { RoleId = roleId };
        }
    }
}
