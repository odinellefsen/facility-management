using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacilityManagement.Models
{
    public class Facility
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public string OwnerId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; } = null!;

        public virtual ICollection<StorageUnit> StorageUnits { get; set; } = new List<StorageUnit>();
    }
}
