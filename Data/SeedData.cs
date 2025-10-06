using FacilityManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace FacilityManagement.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(FacilityManagementContext context, UserManager<ApplicationUser> userManager)
        {
            // Check if data already exists
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            // Create sample users
            var users = new[]
            {
                new ApplicationUser
                {
                    UserName = "john.smith@email.com",
                    Email = "john.smith@email.com",
                    FirstName = "John",
                    LastName = "Smith",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "sarah.johnson@email.com",
                    Email = "sarah.johnson@email.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "mike.wilson@email.com",
                    Email = "mike.wilson@email.com",
                    FirstName = "Mike",
                    LastName = "Wilson",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "emma.davis@email.com",
                    Email = "emma.davis@email.com",
                    FirstName = "Emma",
                    LastName = "Davis",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    EmailConfirmed = true
                }
            };

            foreach (var user in users)
            {
                await userManager.CreateAsync(user, "Password123!");
            }

            // Get the created users from the database
            var createdUsers = context.Users.ToArray();

            // Create sample facilities
            var facilities = new[]
            {
                new Facility
                {
                    Name = "Downtown Storage Center",
                    Description = "Modern storage facility in the heart of downtown with 24/7 access and security.",
                    Address = "123 Main Street",
                    City = "New York",
                    PostalCode = "10001",
                    Country = "USA",
                    OwnerId = createdUsers[0].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Facility
                {
                    Name = "Suburban Storage Solutions",
                    Description = "Family-friendly storage facility with easy parking and ground-level units.",
                    Address = "456 Oak Avenue",
                    City = "Brooklyn",
                    PostalCode = "11201",
                    Country = "USA",
                    OwnerId = createdUsers[1].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Facility
                {
                    Name = "Industrial Storage Complex",
                    Description = "Large-scale storage facility perfect for business and commercial use.",
                    Address = "789 Industrial Blvd",
                    City = "Queens",
                    PostalCode = "11101",
                    Country = "USA",
                    OwnerId = createdUsers[0].Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            context.Facilities.AddRange(facilities);
            context.SaveChanges();

            // Create sample storage units
            var storageUnits = new List<StorageUnit>();

            // Units for Downtown Storage Center
            for (int i = 1; i <= 10; i++)
            {
                storageUnits.Add(new StorageUnit
                {
                    UnitNumber = $"A{i:D2}",
                    Description = $"Small storage unit - {i * 5} sq meters",
                    SizeSquareMeters = i * 5,
                    MonthlyPrice = 50 + (i * 10),
                    FacilityId = facilities[0].Id,
                    IsOccupied = i <= 6, // First 6 units are occupied
                    OccupantId = i <= 6 ? createdUsers[(i - 1) % createdUsers.Length].Id : null,
                    OccupiedAt = i <= 6 ? DateTime.UtcNow.AddDays(-i * 2) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                });
            }

            // Units for Suburban Storage Solutions
            for (int i = 1; i <= 8; i++)
            {
                storageUnits.Add(new StorageUnit
                {
                    UnitNumber = $"B{i:D2}",
                    Description = $"Medium storage unit - {i * 8} sq meters",
                    SizeSquareMeters = i * 8,
                    MonthlyPrice = 80 + (i * 15),
                    FacilityId = facilities[1].Id,
                    IsOccupied = i <= 3, // First 3 units are occupied
                    OccupantId = i <= 3 ? createdUsers[(i - 1) % createdUsers.Length].Id : null,
                    OccupiedAt = i <= 3 ? DateTime.UtcNow.AddDays(-i * 3) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                });
            }

            // Units for Industrial Storage Complex
            for (int i = 1; i <= 6; i++)
            {
                storageUnits.Add(new StorageUnit
                {
                    UnitNumber = $"C{i:D2}",
                    Description = $"Large storage unit - {i * 15} sq meters",
                    SizeSquareMeters = i * 15,
                    MonthlyPrice = 150 + (i * 25),
                    FacilityId = facilities[2].Id,
                    IsOccupied = i <= 2, // First 2 units are occupied
                    OccupantId = i <= 2 ? createdUsers[(i - 1) % createdUsers.Length].Id : null,
                    OccupiedAt = i <= 2 ? DateTime.UtcNow.AddDays(-i * 4) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                });
            }

            context.StorageUnits.AddRange(storageUnits);
            context.SaveChanges();
        }
    }
}
