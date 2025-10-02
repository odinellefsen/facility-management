using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacilityManagement.Models
{
    public class StorageUnit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UnitNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Size must be greater than 0")]
        public double SizeSquareMeters { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPrice { get; set; }

        public bool IsOccupied { get; set; } = false;

        public DateTime? OccupiedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int FacilityId { get; set; }
        public int? OccupantId { get; set; }

        // Navigation properties
        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; } = null!;

        [ForeignKey("OccupantId")]
        public virtual User? Occupant { get; set; }
    }
}
