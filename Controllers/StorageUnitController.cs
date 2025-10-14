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

        // GET: StorageUnit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(m => m.Id == id && m.Facility.OwnerId == currentUser.Id);

            if (storageUnit == null)
            {
                return NotFound("Storage unit not found or you don't have permission to view it.");
            }

            return View(storageUnit);
        }

        // GET: StorageUnit/Create
        public async Task<IActionResult> Create(int? facilityId)
        {
            if (facilityId.HasValue)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge();
                }

                var facility = await _context.Facilities
                    .Where(f => f.Id == facilityId.Value && f.OwnerId == currentUser.Id)
                    .FirstOrDefaultAsync();

                if (facility == null)
                {
                    return NotFound("Facility not found or you don't have permission to add storage units to it.");
                }

                ViewBag.SelectedFacilityId = facilityId;
                ViewBag.FacilityName = facility.Name;
            }

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
                return RedirectToAction("Details", "Facility", new { id = storageUnit.FacilityId });
            }

            // Reload facility information for the view in case of validation errors
            if (storageUnit.FacilityId > 0)
            {
                var facility = await _context.Facilities.FindAsync(storageUnit.FacilityId);
                if (facility != null)
                {
                    ViewBag.SelectedFacilityId = storageUnit.FacilityId;
                    ViewBag.FacilityName = facility.Name;
                }
            }
            return View(storageUnit);
        }

        // GET: StorageUnit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(s => s.Id == id && s.Facility.OwnerId == currentUser.Id);

            if (storageUnit == null)
            {
                return NotFound("Storage unit not found or you don't have permission to edit it.");
            }

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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Remove validation for navigation properties
            ModelState.Remove("Facility");
            ModelState.Remove("Occupant");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing unit to verify ownership and preserve critical data
                    var existingUnit = await _context.StorageUnits
                        .Include(s => s.Facility)
                        .FirstOrDefaultAsync(s => s.Id == id && s.Facility.OwnerId == currentUser.Id);

                    if (existingUnit == null)
                    {
                        return NotFound("Storage unit not found or you don't have permission to edit it.");
                    }

                    // Update only the editable fields on the tracked entity
                    existingUnit.UnitNumber = storageUnit.UnitNumber;
                    existingUnit.Description = storageUnit.Description;
                    existingUnit.SizeSquareMeters = storageUnit.SizeSquareMeters;
                    existingUnit.MonthlyPrice = storageUnit.MonthlyPrice;

                    // Save changes (existingUnit is already tracked)
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Facility", new { id = existingUnit.FacilityId });
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
            }

            // If we get here, validation failed - reload the storage unit with includes for the view
            var reloadedUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(s => s.Id == id);

            return View(reloadedUnit ?? storageUnit);
        }

        // POST: StorageUnit/Occupy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Occupy(int id, string occupantId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Verify the occupantId matches the current user (prevent occupying for someone else)
            if (occupantId != currentUser.Id)
            {
                return Forbid("You can only occupy storage units for yourself.");
            }

            var storageUnit = await _context.StorageUnits.FindAsync(id);
            if (storageUnit == null || storageUnit.IsOccupied)
            {
                return NotFound("Storage unit not found or already occupied.");
            }

            storageUnit.IsOccupied = true;
            storageUnit.OccupantId = occupantId;
            storageUnit.OccupiedAt = DateTime.UtcNow;

            _context.Update(storageUnit);
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // POST: StorageUnit/Vacate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vacate(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (storageUnit == null || !storageUnit.IsOccupied)
            {
                return NotFound("Storage unit not found or not occupied.");
            }

            // Only allow vacating if user is the occupant OR the facility owner
            if (storageUnit.OccupantId != currentUser.Id && storageUnit.Facility.OwnerId != currentUser.Id)
            {
                return Forbid("You can only vacate storage units you occupy or own.");
            }

            storageUnit.IsOccupied = false;
            storageUnit.OccupantId = null;
            storageUnit.OccupiedAt = null;

            _context.Update(storageUnit);
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // GET: StorageUnit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .Include(s => s.Occupant)
                .FirstOrDefaultAsync(m => m.Id == id && m.Facility.OwnerId == currentUser.Id);

            if (storageUnit == null)
            {
                return NotFound("Storage unit not found or you don't have permission to delete it.");
            }

            return View(storageUnit);
        }

        // POST: StorageUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var storageUnit = await _context.StorageUnits
                .Include(s => s.Facility)
                .FirstOrDefaultAsync(s => s.Id == id && s.Facility.OwnerId == currentUser.Id);

            if (storageUnit == null)
            {
                return NotFound("Storage unit not found or you don't have permission to delete it.");
            }

            _context.StorageUnits.Remove(storageUnit);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Facility", new { id = storageUnit.FacilityId });
        }

        private bool StorageUnitExists(int id)
        {
            return _context.StorageUnits.Any(e => e.Id == id);
        }
    }
}
