using System.ComponentModel.DataAnnotations;

namespace FacilityManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Facility> OwnedFacilities { get; set; } = new List<Facility>();
        public virtual ICollection<StorageUnit> OccupiedUnits { get; set; } = new List<StorageUnit>();
    }
}
