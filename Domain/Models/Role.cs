using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class Role : IdentityRole<int>
    {
        public Role()
        {
            AspNetUserRoles = new HashSet<UserRole>();
            this.ArabicName = string.Empty;
        }
        public string ArabicName { get; set; }
        public ICollection<UserRole> AspNetUserRoles { get; set; }


        public static Role Admin { get; } = new()
        {
            Id = 1,
            Name = "Admin",
            ArabicName = "أدمن",
            NormalizedName = "ADMIN"
        };
        public static Role DeliveryMan { get; } = new()
        {
            Id = 2,
            Name = "DeliveryMan",
            ArabicName = "الطيار",
            NormalizedName = "DELIVERYMAN"
        };
        public static Role Customer { get; } = new()
        {
            Id = 3,
            Name = "Customer",
            ArabicName = "العميل",
            NormalizedName = "Customer"
        };
       
        public static IReadOnlyCollection<Role> List =
        [
            Admin,
            DeliveryMan,
            Customer
           
        ];

        public static Role? GetRole(int roleId) => List.FirstOrDefault(r => r.Id == roleId);
        public static Role? GetRole(string roleName) => List.FirstOrDefault(r => r.Name == roleName);
        public static bool Exists(int roleId) => List.Any(r => r.Id == roleId);


        public override string ToString()
        {
            return $"Role Name:{Name}";
        }
    }
}
