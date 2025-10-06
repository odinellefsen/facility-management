using FacilityManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FacilityManagement.Data
{
    public class FacilityManagementContext : IdentityDbContext<ApplicationUser>
    {
        public FacilityManagementContext(DbContextOptions<FacilityManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Facility> Facilities { get; set; }
        public DbSet<StorageUnit> StorageUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser entity
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            });

            // Configure Facility entity
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);

                // Configure relationship with ApplicationUser (Owner)
                entity.HasOne(f => f.Owner)
                    .WithMany(u => u.OwnedFacilities)
                    .HasForeignKey(f => f.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure StorageUnit entity
            modelBuilder.Entity<StorageUnit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(18,2)");

                // Configure relationship with Facility
                entity.HasOne(su => su.Facility)
                    .WithMany(f => f.StorageUnits)
                    .HasForeignKey(su => su.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure relationship with ApplicationUser (Occupant)
                entity.HasOne(su => su.Occupant)
                    .WithMany(u => u.OccupiedUnits)
                    .HasForeignKey(su => su.OccupantId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Ensure unique unit numbers within a facility
                entity.HasIndex(e => new { e.FacilityId, e.UnitNumber }).IsUnique();
            });
        }
    }
}
