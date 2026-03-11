# Customer Location Tracking Implementation Guide

## Overview

This document provides a comprehensive guide for implementing location tracking for customers. The system allows customers to save and update their current location (latitude and longitude), similar to how delivery men track their locations. This is useful for order delivery, finding nearby services, and location-based features.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain Models](#domain-models)
3. [Database Schema](#database-schema)
4. [Entity Relationships](#entity-relationships)
5. [Commands](#commands)
6. [Queries](#queries)
7. [DTOs](#dtos)
8. [Controller Endpoints](#controller-endpoints)
9. [Entity Mapping Configuration](#entity-mapping-configuration)
10. [Implementation Examples](#implementation-examples)
11. [Best Practices](#best-practices)

---

## Architecture Overview

The customer location tracking system consists of:

- **Customer_Location Entity**: Stores latitude and longitude for each customer
- **Customer Model**: Contains a navigation property to Customer_Location
- **SaveCustomerLocationCommand**: Command to save/update customer location
- **GetCustomerLocationQuery**: Query to retrieve customer location
- **API Endpoint**: RESTful endpoint to save location from mobile app

### Flow Diagram

```
Mobile App → Get GPS Coordinates → API Endpoint → SaveCustomerLocationCommand
                                                      ↓
                                            Customer.SaveLocation() → Update/Create Customer_Location
                                                      ↓
                                            Database Save → Customer_Location Table
```

---

## Domain Models

### 1. Customer_Location Entity

**File**: `Domain/Models/Customer_Location.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Customer_Location
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }

        /// <summary>
        /// Creates a new Customer_Location instance
        /// </summary>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <returns>New Customer_Location instance</returns>
        public static Customer_Location Instance(
            double longitude,
            double latitude)
        {
            return new Customer_Location
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        /// <summary>
        /// Updates the location coordinates
        /// </summary>
        /// <param name="longitude">New longitude coordinate</param>
        /// <param name="latitude">New latitude coordinate</param>
        public void UpdateLocation(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
    }
}
```

**Key Points**:
- `Id`: Primary key, auto-generated
- `CustomerId`: Foreign key to Customer table
- `Longitude`: Longitude coordinate (double)
- `Latitude`: Latitude coordinate (double)
- `Instance()`: Factory method to create new location
- `UpdateLocation()`: Method to update existing location coordinates

### 2. Customer Model Modification

**File**: `Domain/Models/Customer.cs`

Add the following property to the Customer class:

```csharp
public class Customer
{
    // ... existing properties ...
    
    /// <summary>
    /// Navigation property to Customer_Location
    /// One-to-One relationship: One Customer has one Customer_Location
    /// </summary>
    public Customer_Location Customer_Location { get; private set; }
    
    // ... rest of the class ...
    
    /// <summary>
    /// Saves or updates the customer's location
    /// </summary>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="latitude">Latitude coordinate</param>
    public void SaveLocation(double longitude, double latitude)
    {
        var location = this.Customer_Location;
        if (location is null)
        {
            // Create new location if it doesn't exist
            var newLocation = Customer_Location.Instance(longitude, latitude);
            this.Customer_Location = newLocation;
            return;
        }

        // Update existing location
        location.UpdateLocation(longitude, latitude);
    }
}
```

**Key Points**:
- `Customer_Location`: Navigation property (one-to-one relationship)
- `SaveLocation()`: Method that handles both creation and update
- If location doesn't exist, creates a new one
- If location exists, updates the coordinates

---

## Database Schema

### Customer_Location Table

```sql
CREATE TABLE Customer_Locations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    Longitude FLOAT NOT NULL,
    Latitude FLOAT NOT NULL,
    
    CONSTRAINT FK_Customer_Locations_NA_Customers_CustomerId 
        FOREIGN KEY (CustomerId) 
        REFERENCES NA_Customers(Id) 
        ON DELETE CASCADE,
    
    CONSTRAINT UQ_Customer_Locations_CustomerId 
        UNIQUE (CustomerId)  -- Ensures one location per customer
);

-- Index for faster lookups
CREATE INDEX IX_Customer_Locations_CustomerId 
    ON Customer_Locations(CustomerId);
```

**Table Name**: `Customer_Locations`

**Columns**:
- `Id`: Primary key (auto-increment)
- `CustomerId`: Foreign key to `NA_Customers` table (unique, one-to-one)
- `Longitude`: Longitude coordinate (double/float)
- `Latitude`: Latitude coordinate (double/float)

**Constraints**:
- Foreign key constraint to `NA_Customers` table
- Unique constraint on `CustomerId` (ensures one location per customer)
- Cascade delete (if customer is deleted, location is also deleted)

---

## Entity Relationships

### Relationship Diagram

```
Customer (1) ──────── (1) Customer_Location
   │                        │
   │                        │
   └─── CustomerId ──────────┘
```

### Relationship Details

- **Type**: One-to-One (1:1)
- **Customer → Customer_Location**: One customer has exactly one location
- **Customer_Location → Customer**: One location belongs to exactly one customer
- **Foreign Key**: `Customer_Location.CustomerId` references `Customer.Id`
- **Navigation Property**: `Customer.Customer_Location`

---

## Commands

### SaveCustomerLocationCommand

```csharp
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Location.Commands
{
    /// <summary>
    /// Command to save or update customer location
    /// </summary>
    public sealed record SaveCustomerLocationCommand : IRequest<Result>
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        private class SaveCustomerLocationCommandHandler :
            IRequestHandler<SaveCustomerLocationCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public SaveCustomerLocationCommandHandler(
                INaqlahContext context,
                IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result> Handle(
                SaveCustomerLocationCommand request, 
                CancellationToken cancellationToken)
            {
                // Get customer with location (if exists)
                var customer = await context.Customers
                    .Include(x => x.Customer_Location)
                    .AsTracking()  // Enable change tracking
                    .FirstOrDefaultAsync(x => x.UserId == userSession.UserId, cancellationToken);

                if (customer is null)
                {
                    return Result.Failure("Customer Not Found");
                }

                // Save or update location
                customer.SaveLocation(request.Longitude, request.Latitude);

                // Save changes to database
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}
```

**Key Points**:
- Uses `IUserSession` to get the current logged-in user
- Includes `Customer_Location` in the query to load existing location
- Uses `AsTracking()` to enable Entity Framework change tracking
- Calls `customer.SaveLocation()` which handles create/update logic
- Returns `Result` for error handling

---

## Queries

### GetCustomerLocationQuery

```csharp
using Application.Features.CustomerSection.Feature.Location.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Location.Queries
{
    /// <summary>
    /// Query to get customer's current location
    /// </summary>
    public sealed record GetCustomerLocationQuery : IRequest<Result<CustomerLocationDto>>
    {
        private class GetCustomerLocationQueryHandler :
            IRequestHandler<GetCustomerLocationQuery, Result<CustomerLocationDto>>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public GetCustomerLocationQueryHandler(
                INaqlahContext context,
                IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }

            public async Task<Result<CustomerLocationDto>> Handle(
                GetCustomerLocationQuery request, 
                CancellationToken cancellationToken)
            {
                var customer = await context.Customers
                    .Include(c => c.Customer_Location)
                    .FirstOrDefaultAsync(c => c.UserId == userSession.UserId, cancellationToken);

                if (customer is null)
                {
                    return Result.Failure<CustomerLocationDto>("Customer Not Found");
                }

                if (customer.Customer_Location is null)
                {
                    return Result.Failure<CustomerLocationDto>("Customer location not available");
                }

                var locationDto = new CustomerLocationDto
                {
                    CustomerId = customer.Id,
                    Longitude = customer.Customer_Location.Longitude,
                    Latitude = customer.Customer_Location.Latitude
                };

                return Result.Success(locationDto);
            }
        }
    }
}
```


**File**: `Application/Features/CustomerSection/Feature/Location/Queries/GetCustomersWithinRadiusQuery.cs`

```csharp
using Application.Features.CustomerSection.Feature.Location.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Location.Queries
{
    /// <summary>
    /// Query to get customers within a specified radius from a point
    /// </summary>
    public sealed record GetCustomersWithinRadiusQuery : IRequest<Result<List<CustomerLocationDto>>>
    {
        public double CenterLatitude { get; set; }
        public double CenterLongitude { get; set; }
        public double RadiusInKm { get; set; } = 5.0; // Default 5km radius

        private class GetCustomersWithinRadiusQueryHandler :
            IRequestHandler<GetCustomersWithinRadiusQuery, Result<List<CustomerLocationDto>>>
        {
            private readonly INaqlahContext context;
            private const double EARTH_RADIUS_KM = 6371.0; // Earth's radius in kilometers

            public GetCustomersWithinRadiusQueryHandler(INaqlahContext context)
            {
                this.context = context;
            }

            public async Task<Result<List<CustomerLocationDto>>> Handle(
                GetCustomersWithinRadiusQuery request, 
                CancellationToken cancellationToken)
            {
                var customers = await context.Customers
                    .Include(c => c.Customer_Location)
                    .Where(c => c.Customer_Location != null)
                    .ToListAsync(cancellationToken);

                var customersWithinRadius = new List<CustomerLocationDto>();

                foreach (var customer in customers)
                {
                    var distance = CalculateDistance(
                        request.CenterLatitude,
                        request.CenterLongitude,
                        customer.Customer_Location.Latitude,
                        customer.Customer_Location.Longitude
                    );

                    if (distance <= request.RadiusInKm)
                    {
                        customersWithinRadius.Add(new CustomerLocationDto
                        {
                            CustomerId = customer.Id,
                            Longitude = customer.Customer_Location.Longitude,
                            Latitude = customer.Customer_Location.Latitude
                        });
                    }
                }

                return Result.Success(customersWithinRadius);
            }

            /// <summary>
            /// Calculate distance between two coordinates using Haversine formula
            /// </summary>
            private double CalculateDistance(
                double lat1, double lon1, 
                double lat2, double lon2)
            {
                var dLat = ToRadians(lat2 - lat1);
                var dLon = ToRadians(lon2 - lon1);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = EARTH_RADIUS_KM * c;

                return distance;
            }

            private double ToRadians(double degrees)
            {
                return degrees * Math.PI / 180.0;
            }
        }
    }
}
```

---

## DTOs

### CustomerLocationDto

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Location.Dtos
{
    /// <summary>
    /// Data Transfer Object for Customer Location
    /// </summary>
    public class CustomerLocationDto
    {
        public int CustomerId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
```

### CustomerLocationRequest


```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Location.Dtos
{
    /// <summary>
    /// Request DTO for saving customer location
    /// </summary>
    public class CustomerLocationRequest
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
```

---

## Controller Endpoints

### CustomerController - Save Location Endpoint


Add the following endpoint:

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
[Route("SaveLocation")]
[Authorize]  // Require authentication
public async Task<IActionResult> SaveLocation([FromBody] CustomerLocationRequest request)
{
    var result = await mediator.Send(new SaveCustomerLocationCommand
    {
        Longitude = request.Longitude,
        Latitude = request.Latitude
    });

    if (result.IsFailure)
    {
        return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
    }
    return Ok();
}
```

### CustomerController - Get Location Endpoint

```csharp
[HttpGet]
[ProducesResponseType(typeof(CustomerLocationDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
[Route("GetLocation")]
[Authorize]  // Require authentication
public async Task<IActionResult> GetLocation()
{
    var result = await mediator.Send(new GetCustomerLocationQuery());

    if (result.IsFailure)
    {
        return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
    }
    return Ok(result.Value);
}
```

**API Endpoints**:
- `POST /api/Customer/SaveLocation` - Save or update customer location
- `GET /api/Customer/GetLocation` - Get current customer location

**Request Body** (SaveLocation):
```json
{
  "longitude": 31.2001,
  "latitude": 29.9245
}
```

**Response** (GetLocation):
```json
{
  "customerId": 123,
  "longitude": 31.2001,
  "latitude": 29.9245
}
```

---

## Entity Mapping Configuration

### Customer_Location Mapping


```csharp
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
    public class Customer_LocationMapping : IEntityTypeConfiguration<Customer_Location>
    {
        public void Configure(EntityTypeBuilder<Customer_Location> builder)
        {
            builder.ToTable("Customer_Locations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            builder.Property(x => x.CustomerId).IsRequired();
            builder.Property(x => x.Longitude).IsRequired().HasColumnType("float");
            builder.Property(x => x.Latitude).IsRequired().HasColumnType("float");

            // Create index on CustomerId for faster lookups
            builder.HasIndex(x => x.CustomerId).IsUnique();
        }
    }
}
```

### Customer Mapping Update

**File**: `Infrastructure/MappingConfigurations/CustomerMapping.cs`

Add the relationship configuration:

```csharp
public class CustomerMapping : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("NA_Customers");
        builder.HasKey(x => x.Id);
        
        // ... existing configurations ...
        
        // One-to-One relationship with Customer_Location
        builder.HasOne(x => x.Customer_Location)
               .WithOne()
               .HasForeignKey<Customer_Location>(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);  // Cascade delete
    }
}
```

### Register in DbContext

```csharp
public class NaqlahContext : DbContext, INaqlahContext
{
    // ... existing DbSets ...
    
    public DbSet<Customer_Location> Customer_Locations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Customer_LocationMapping).Assembly);
        // ... other configurations ...
    }
}
```

### Register in Interface

**File**: `Domain/InterFaces/INaqlahContext.cs`

```csharp
public interface INaqlahContext
{
    // ... existing DbSets ...
    
    DbSet<Customer_Location> Customer_Locations { get; set; }
    
    // ... other members ...
}
```

---

## Implementation Examples

### Example 1: Mobile App Saves Location

**Mobile App (Flutter/Dart Example)**:

```dart
import 'package:geolocator/geolocator.dart';

// Get current location
Position position = await Geolocator.getCurrentPosition(
  locationSettings: LocationSettings(
    accuracy: LocationAccuracy.high,
  ),
);

// Send to backend
await apiClient.saveLocation(
  longitude: position.longitude,
  latitude: position.latitude,
);
```

**API Call**:
```http
POST /api/Customer/SaveLocation
Authorization: Bearer {token}
Content-Type: application/json

{
  "longitude": 31.2001,
  "latitude": 29.9245
}
```

### Example 2: Get Customer Location

**Backend Code**:
```csharp
// In a service or command handler
var locationQuery = new GetCustomerLocationQuery();
var result = await mediator.Send(locationQuery);

if (result.IsSuccess)
{
    var location = result.Value;
    Console.WriteLine($"Customer {location.CustomerId} is at: {location.Latitude}, {location.Longitude}");
}
```

### Example 3: Find Customers Near a Point

```csharp
var query = new GetCustomersWithinRadiusQuery
{
    CenterLatitude = 29.9245,
    CenterLongitude = 31.2001,
    RadiusInKm = 10.0  // 10km radius
};

var result = await mediator.Send(query);
if (result.IsSuccess)
{
    var nearbyCustomers = result.Value;
    // Process nearby customers
}
```

### Example 4: Update Location Periodically

**Mobile App Background Service**:
```dart
// Update location every 30 seconds when app is active
Timer.periodic(Duration(seconds: 30), (timer) async {
  Position position = await Geolocator.getCurrentPosition();
  await apiClient.saveLocation(
    longitude: position.longitude,
    latitude: position.latitude,
  );
});
```

---

## Best Practices

### 1. Location Accuracy

- **Use high accuracy**: Request high accuracy GPS coordinates
- **Handle permissions**: Ensure location permissions are granted
- **Error handling**: Handle cases where GPS is unavailable
- **Battery optimization**: Balance update frequency with battery usage

### 2. Update Frequency

- **On app launch**: Update location when app starts
- **Periodic updates**: Update every 30-60 seconds when app is active
- **On significant change**: Update when location changes significantly (e.g., >100 meters)
- **Background updates**: Consider background location updates for critical features

### 3. Data Validation

```csharp
public void SaveLocation(double longitude, double latitude)
{
    // Validate coordinates
    if (longitude < -180 || longitude > 180)
        throw new ArgumentException("Invalid longitude");
    
    if (latitude < -90 || latitude > 90)
        throw new ArgumentException("Invalid latitude");
    
    // ... rest of implementation
}
```

### 4. Performance

- **Index CustomerId**: Ensure CustomerId is indexed for fast lookups
- **Use Include()**: Always include Customer_Location when querying
- **Batch updates**: Consider batching location updates if needed
- **Caching**: Cache location data if frequently accessed

### 5. Security

- **Authentication**: Always require authentication for location endpoints
- **Authorization**: Ensure users can only update their own location
- **Privacy**: Consider privacy settings (allow users to disable location tracking)
- **Data retention**: Consider data retention policies for location history

### 6. Error Handling

```csharp
public async Task<Result> Handle(SaveCustomerLocationCommand request, CancellationToken cancellationToken)
{
    try
    {
        // Validate input
        if (request.Longitude < -180 || request.Longitude > 180)
            return Result.Failure("Invalid longitude value");
        
        if (request.Latitude < -90 || request.Latitude > 90)
            return Result.Failure("Invalid latitude value");
        
        // ... rest of implementation
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error saving customer location");
        return Result.Failure("An error occurred while saving location");
    }
}
```

### 7. Testing

- **Unit tests**: Test Customer_Location entity methods
- **Integration tests**: Test command handlers and queries
- **API tests**: Test controller endpoints
- **Edge cases**: Test with invalid coordinates, null values, etc.

---

## Migration

### Create Migration

```bash
dotnet ef migrations add AddCustomer_LocationTable --project Infrastructure --startup-project NAQLAH.Server
```

### Migration File Example

```csharp
public partial class AddCustomer_LocationTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customer_Locations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerId = table.Column<int>(type: "int", nullable: false),
                Longitude = table.Column<double>(type: "float", nullable: false),
                Latitude = table.Column<double>(type: "float", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customer_Locations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Customer_Locations_NA_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "NA_Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.UniqueConstraint("UQ_Customer_Locations_CustomerId", x => x.CustomerId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Customer_Locations_CustomerId",
            table: "Customer_Locations",
            column: "CustomerId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Customer_Locations");
    }
}
```

---

## Summary

This implementation provides a complete customer location tracking system:

1. **Customer_Location Entity**: Stores latitude and longitude for each customer
2. **One-to-One Relationship**: Each customer has exactly one location
3. **SaveCustomerLocationCommand**: Handles saving/updating customer location
4. **GetCustomerLocationQuery**: Retrieves customer's current location
5. **API Endpoints**: RESTful endpoints for mobile app integration
6. **Entity Mapping**: Proper EF Core configuration for relationships
7. **Database Schema**: Optimized table structure with indexes

**Key Features**:
- Automatic create/update logic
- One location per customer
- Cascade delete support
- Indexed for performance
- Full CRUD operations
- Distance calculation support

For questions or issues, refer to the Entity Framework Core documentation and the DeliveryMan location implementation as a reference.



