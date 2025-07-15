using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            this.ActivationCode = string.Empty;
            AspNetUserClaims = new HashSet<UserClaim>();
            AspNetUserLogins = new HashSet<UserLogin>();
            AspNetUserRoles = new HashSet<UserRole>();
        }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string ActivationCode { get; set; }
        public ICollection<UserClaim> AspNetUserClaims { get; set; }

        public ICollection<UserLogin> AspNetUserLogins { get; set; }

        public ICollection<UserRole> AspNetUserRoles { get; set; }
        public DeliveryMan? DeliveryMan { get; private set; }


        public static Result<User> CreateDeliveryUser(string mobile,
                                                     string email,
                                                     string name)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return Result.Failure<User>("Phone Number Is Required");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<User>("Email Is Required");
            }

            var deliveryMan = DeliveryMan.Instance(mobile, name);
            var deliveryManRoleId = Role.DeliveryMan.Id;
            var userRole = UserRole.Instance(deliveryManRoleId);

            var user = new User
            {
                PhoneNumber = mobile,
                Email = email,
                UserName = mobile,
                IsActive = false,
                IsDeleted = false,
                DeliveryMan = deliveryMan
            };

            user.AspNetUserRoles.Add(userRole);
            return user;
        }


        public void GenerateActivationCode()
        {
            var random=new Random();
            var activeCode = random.Next(1000, 10000).ToString();
            this.ActivationCode = activeCode;
        }
    }
}
