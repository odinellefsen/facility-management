using FacilityManagement.Data;
using FacilityManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacilityManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly FacilityManagementContext _context;

        public HomeController(FacilityManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                Message = "Welcome to Facility Management System!",
                Today = DateTime.Now,
                TotalFacilities = await _context.Facilities.CountAsync(),
                TotalStorageUnits = await _context.StorageUnits.CountAsync(),
                OccupiedUnits = await _context.StorageUnits.CountAsync(su => su.IsOccupied),
                TotalUsers = await _context.Users.CountAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Admin()
        {
            var facilities = await _context.Facilities
                .Include(f => f.Owner)
                .Include(f => f.StorageUnits)
                .ToListAsync();

            return View(facilities);
        }

        public async Task<IActionResult> Browse()
        {
            var facilities = await _context.Facilities
                .Include(f => f.Owner)
                .Include(f => f.StorageUnits.Where(su => !su.IsOccupied))
                .ToListAsync();

            return View(facilities);
        }
    }
}