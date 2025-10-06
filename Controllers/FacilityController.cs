using FacilityManagement.Data;
using FacilityManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacilityManagement.Controllers
{
    [Authorize]
    public class FacilityController : Controller
    {
        private readonly FacilityManagementContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacilityController(FacilityManagementContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Facility
        public async Task<IActionResult> Index()
        {
            var facilities = await _context.Facilities
                .Include(f => f.Owner)
                .Include(f => f.StorageUnits)
                .ToListAsync();
            return View(facilities);
        }

        // GET: Facility/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .Include(f => f.Owner)
                .Include(f => f.StorageUnits)
                    .ThenInclude(su => su.Occupant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (facility == null)
            {
                return NotFound();
            }

            return View(facility);
        }

        // GET: Facility/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facility/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Address,City,PostalCode,Country")] Facility facility)
        {
            // Automatically set the current user as the owner BEFORE validation
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                facility.OwnerId = currentUser.Id;
            }
            else
            {
                // If no user is found, this is a problem
                ModelState.AddModelError("", "Unable to determine current user.");
                return View(facility);
            }

            // Remove any validation errors for OwnerId and Owner since we set it programmatically
            ModelState.Remove("OwnerId");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                facility.CreatedAt = DateTime.UtcNow;
                _context.Add(facility);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(facility);
        }

        // GET: Facility/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            ViewBag.Users = _context.Users.ToList();
            return View(facility);
        }

        // POST: Facility/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Address,City,PostalCode,Country,OwnerId,CreatedAt")] Facility facility)
        {
            if (id != facility.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facility);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(facility.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = _context.Users.ToList();
            return View(facility);
        }

        // GET: Facility/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .Include(f => f.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (facility == null)
            {
                return NotFound();
            }

            return View(facility);
        }

        // POST: Facility/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(e => e.Id == id);
        }
    }
}
