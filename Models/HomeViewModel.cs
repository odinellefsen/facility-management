namespace FacilityManagement.Models
{
    public class HomeViewModel
    {
        public string Message { get; set; } = "Welcome to Facility Management System!";
        public DateTime Today { get; set; } = DateTime.Now;
        public int TotalFacilities { get; set; }
        public int TotalStorageUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int AvailableUnits => TotalStorageUnits - OccupiedUnits;
        public int TotalUsers { get; set; }
        public double OccupancyRate => TotalStorageUnits > 0 ? (double)OccupiedUnits / TotalStorageUnits * 100 : 0;
    }
}
