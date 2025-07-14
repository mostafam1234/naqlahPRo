using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class User:IdentityUser<int>
    {
        public User()
        {
            this.ActivationCode = string.Empty;
            AspNetUserClaims = new HashSet<UserClaim>();
            AspNetUserLogins = new HashSet<UserLogin>();
            AspNetUserRoles = new HashSet<UserRole>();
        }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; private set; }
        public string ActivationCode { get;private set; }
        public ICollection<UserClaim> AspNetUserClaims { get; set; }

        public ICollection<UserLogin> AspNetUserLogins { get; set; }

        public ICollection<UserRole> AspNetUserRoles { get; set; }
        public DeliveryMan? DeliveryMan { get;private set; }
    }
}
