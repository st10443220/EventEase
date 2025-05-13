using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobService _blobService;

        public VenuesController(ApplicationDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,VenueLocation,VenueCapacity,VenueImageUrl,ImageUpload")] Venue venue)
        {
            if (venue.ImageUpload == null || venue.ImageUpload.Length == 0)
            {
                ModelState.AddModelError(nameof(venue.ImageUpload), "Please upload an image file.");
            }

            if (ModelState.IsValid)
            {
                var image = venue.ImageUpload;

                if (image is { Length: > 0 })
                {
                    var url = await _blobService.UploadAsync(image.OpenReadStream(), Path.GetRandomFileName() + Path.GetExtension(image.FileName), image.ContentType);
                    venue.VenueImageUrl = url;
                    ViewBag.Msg = "Upload Successful";
                }
                else
                {
                    ViewBag.Msg = "Select a valid file.";
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }



        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,VenueLocation,VenueCapacity,VenueImageUrl,ImageUpload")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var image = venue.ImageUpload;

                    if (image is { Length: > 0 })
                    {
                        var url = await _blobService.UploadAsync(image.OpenReadStream(), Path.GetRandomFileName() + Path.GetExtension(image.FileName), image.ContentType);
                        venue.VenueImageUrl = url;
                        ViewBag.Msg = "Upload Successful";
                    }
                    else
                    {
                        // Keep the existing image if no new image is uploaded
                        var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                        if (existingVenue != null)
                        {
                            venue.VenueImageUrl = existingVenue.VenueImageUrl;
                        }
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(venue);
        }



        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            bool hasBookings = await _context.Bookings.AnyAsync(g => g.VenueId == id);

            if (hasBookings)
            {
                ModelState.AddModelError("", "Cannot delete the venue because there are existing bookings associated with it.");
                return View("Delete", venue);
            }

            if (!string.IsNullOrEmpty(venue.VenueImageUrl))
            {
                bool blobDeleted = await _blobService.DeleteAsync(venue.VenueImageUrl);
                if (!blobDeleted)
                {
                    Console.WriteLine($"Failed to delete blob for venue {id}: {venue.VenueImageUrl}");
                    TempData["Warning"] = "Venue deleted, but failed to delete associated image.";
                }
            }

            // Delete the venue from the database
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}
