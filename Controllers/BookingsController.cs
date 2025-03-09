using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create(int? eventId)
        {
            // Fetch the list of events for the dropdown
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");

            // Fetch the list of venues for the dropdown
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");

            // Fetch event details if an EventId is provided
            if (eventId.HasValue)
            {
                var eventDetails = _context.Events
                    .FirstOrDefault(e => e.EventId == eventId);

                if (eventDetails != null)
                {
                    // Pass event details to the view
                    ViewBag.EventDetails = eventDetails;
                }
            }

            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,StartBookingDate,EndBookingDate,EventId,VenueId")] Booking booking)
        {
            // Fetch the associated event
            var eventDetails = await _context.Events.FindAsync(booking.EventId);

            if (eventDetails == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(booking);
            }

            // Validate booking dates against event dates
            if (booking.StartBookingDate < eventDetails.StartEventDate || booking.StartBookingDate > eventDetails.FinishEventDate)
            {
                ModelState.AddModelError("StartBookingDate", "Start Booking Date must be within the event's date range.");
            }

            if (booking.EndBookingDate < eventDetails.StartEventDate || booking.EndBookingDate > eventDetails.FinishEventDate)
            {
                ModelState.AddModelError("EndBookingDate", "End Booking Date must be within the event's date range.");
            }

            if (booking.EndBookingDate < booking.StartBookingDate)
            {
                ModelState.AddModelError("", "End Booking Date cannot be before Start Booking Date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            if (id.HasValue)
            {
                var eventDetails = _context.Events
                    .FirstOrDefault(e => e.EventId == id);

                if (eventDetails != null)
                {
                    // Pass event details to the view
                    ViewBag.EventDetails = eventDetails;
                }
            }

            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,StartBookingDate,EndBookingDate,EventId,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Fetch the associated event
            var eventDetails = await _context.Events.FindAsync(booking.EventId);

            if (eventDetails == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(booking);
            }

            // Validate booking dates against event dates
            if (booking.StartBookingDate < eventDetails.StartEventDate || booking.StartBookingDate > eventDetails.FinishEventDate)
            {
                ModelState.AddModelError("StartBookingDate", "Start Booking Date must be within the event's date range.");
            }

            if (booking.EndBookingDate < eventDetails.StartEventDate || booking.EndBookingDate > eventDetails.FinishEventDate)
            {
                ModelState.AddModelError("EndBookingDate", "End Booking Date must be within the event's date range.");
            }

            if (booking.EndBookingDate < booking.StartBookingDate)
            {
                ModelState.AddModelError("", "End Booking Date cannot be before Start Booking Date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
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

            // Repopulate dropdowns if validation fails
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
