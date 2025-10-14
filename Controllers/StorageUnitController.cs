using FacilityManagement.Data;
using FacilityManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacilityManagement.Controllers
{
    [Authorize]
    public class StorageUnitController : Controller
    {
        private readonly FacilityManagementContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StorageUnitController(FacilityManagementContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StorageUnit
        public async Task<IActionResult> Index(int? facilityId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            IQueryable<StorageUnit> storageUnits = _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .Where(s => s.Facility.OwnerId == currentUser.Id); // Only show units from user's facilities

            if (facilityId.HasValue)
            {
                storageUnits = storageUnits.Where(s => s.FacilityId == facilityId.Value);
                ViewBag.FacilityName = await _context.Facilities
                    .Where(f => f.Id == facilityId.Value && f.OwnerId == currentUser.Id) // Ensure user owns the facility
                    .Select(f => f.Name)
                    .FirstOrDefaultAsync();
                ViewBag.FacilityId = facilityId.Value;
            }

            return View(await storageUnits.ToListAsync());
        }

        // GET: StorageUnit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storageUnit == null)
            {
                return NotFound();
            }

            return View(storageUnit);
        }

        // GET: StorageUnit/Create
        public async Task<IActionResult> Create(int? facilityId)
        {
            ViewBag.Facilities = await _context.Facilities.ToListAsync();
            ViewBag.SelectedFacilityId = facilityId;
            return View();
        }

        // POST: StorageUnit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitNumber,Description,SizeSquareMeters,MonthlyPrice,FacilityId")] StorageUnit storageUnit)
        {
            // Remove validation errors for navigation properties that we don't bind
            ModelState.Remove("Facility");
            ModelState.Remove("Occupant");

            // Debug: Log model state
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                storageUnit.CreatedAt = DateTime.UtcNow;
                storageUnit.IsOccupied = false;
                _context.Add(storageUnit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { facilityId = storageUnit.FacilityId });
            }
            ViewBag.Facilities = await _context.Facilities.ToListAsync();
            return View(storageUnit);
        }

        // GET: StorageUnit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (storageUnit == null)
            {
                return NotFound();
            }
            ViewBag.Facilities = await _context.Facilities.ToListAsync();
            return View(storageUnit);
        }

        // POST: StorageUnit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UnitNumber,Description,SizeSquareMeters,MonthlyPrice,IsOccupied,OccupiedAt,CreatedAt,FacilityId,OccupantId")] StorageUnit storageUnit)
        {
            if (id != storageUnit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(storageUnit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StorageUnitExists(storageUnit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { facilityId = storageUnit.FacilityId });
            }
            ViewBag.Facilities = await _context.Facilities.ToListAsync();
            return View(storageUnit);
        }

        // POST: StorageUnit/Occupy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Occupy(int id, string occupantId)
        {
            var storageUnit = await _context.StorageUnits.FindAsync(id);
            if (storageUnit == null || storageUnit.IsOccupied)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(occupantId);
            if (user == null)
            {
                return NotFound();
            }

            storageUnit.IsOccupied = true;
            storageUnit.OccupantId = occupantId;
            storageUnit.OccupiedAt = DateTime.UtcNow;

            _context.Update(storageUnit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: StorageUnit/Vacate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vacate(int id)
        {
            var storageUnit = await _context.StorageUnits.FindAsync(id);
            if (storageUnit == null || !storageUnit.IsOccupied)
            {
                return NotFound();
            }

            storageUnit.IsOccupied = false;
            storageUnit.OccupantId = null;
            storageUnit.OccupiedAt = null;

            _context.Update(storageUnit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: StorageUnit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storageUnit == null)
            {
                return NotFound();
            }

            return View(storageUnit);
        }

        // POST: StorageUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var storageUnit = await _context.StorageUnits.FindAsync(id);
            if (storageUnit != null)
            {
                _context.StorageUnits.Remove(storageUnit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StorageUnitExists(int id)
        {
            return _context.StorageUnits.Any(e => e.Id == id);
        }
    }
}
