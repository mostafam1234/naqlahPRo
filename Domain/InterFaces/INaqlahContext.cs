using CSharpFunctionalExtensions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface INaqlahContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<DeliveryMan> DeliveryMen { get; set; }
        DbSet<VehicleBrand> VehicleBrands { get; set; }
        DbSet<VehicleType> VehicleTypes { get; set; }
        DbSet<Resident> Residents { get; set; }
        DbSet<Company> Companies { get; set; }
        DbSet<Renter> Renters { get; set; }
        DbSet<Assistant> Assistants { get; set; }
        DbSet<AssistanWork> AssistanWorks { get; set; }
        DbSet<DeliveryVehicle> DeliveryVehicles { get; set; }
        DbSet<DeliveryManLocation> DeliveryManLocations { get; set; }
        Task<Result> SaveChangesAsyncWithResult();
    }
}
