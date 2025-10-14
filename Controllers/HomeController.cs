using FacilityManagement.Data;
using FacilityManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacilityManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly FacilityManagementContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(FacilityManagementContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        [Authorize]
        public async Task<IActionResult> Admin()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var facilities = await _context.Facilities
                .Include(f => f.Owner)
                .Include(f => f.StorageUnits)
                .Where(f => f.OwnerId == currentUser.Id)
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

        [Authorize]
        public async Task<IActionResult> MyUnits()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var occupiedUnits = await _context.StorageUnits
                .Include(su => su.Facility)
                    .ThenInclude(f => f.Owner)
                .Where(su => su.OccupantId == currentUser.Id)
                .OrderBy(su => su.Facility.Name)
                .ThenBy(su => su.UnitNumber)
                .ToListAsync();

            return View(occupiedUnits);
        }
    }
}