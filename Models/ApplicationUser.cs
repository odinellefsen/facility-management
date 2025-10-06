using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FacilityManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Full name property for display
        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Facility> OwnedFacilities { get; set; } = new List<Facility>();
        public virtual ICollection<StorageUnit> OccupiedUnits { get; set; } = new List<StorageUnit>();
    }
}
