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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");

            if (eventId.HasValue)
            {
                var eventDetails = _context.Events.FirstOrDefault(e => e.EventId == eventId);
                if (eventDetails != null)
                {
                    ViewBag.EventDetails = eventDetails;
                    ViewBag.EventStartDate = eventDetails?.StartEventDate?.ToString("yyyy-MM-dd");
                    ViewBag.EventEndDate = eventDetails?.FinishEventDate?.ToString("yyyy-MM-dd");
                }
            }

            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,StartBookingDate,EndBookingDate,EventId,VenueId")] Booking booking)
        {
            var eventDetails = await _context.Events.FindAsync(booking.EventId);

            if (eventDetails == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(booking);
            }

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

            bool isVenueOverlapping = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.EventId == booking.EventId &&
                               ((booking.StartBookingDate >= b.StartBookingDate && booking.StartBookingDate <= b.EndBookingDate) ||
                                (booking.EndBookingDate >= b.StartBookingDate && booking.EndBookingDate <= b.EndBookingDate) ||
                                (booking.StartBookingDate <= b.StartBookingDate && booking.EndBookingDate >= b.EndBookingDate)));

            if (isVenueOverlapping)
            {
                ModelState.AddModelError("", "This venue already has an overlapping booking for this event.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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

            var eventDetails = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (eventDetails != null)
            {
                ViewBag.EventDetails = eventDetails;
            }

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,StartBookingDate,EndBookingDate,EventId,VenueId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            var eventDetails = await _context.Events.FindAsync(booking.EventId);

            if (eventDetails == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(booking);
            }

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

            bool isVenueOverlapping = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.EventId == booking.EventId &&
                               b.BookingId != booking.BookingId &&
                               ((booking.StartBookingDate >= b.StartBookingDate && booking.StartBookingDate <= b.EndBookingDate) ||
                                (booking.EndBookingDate >= b.StartBookingDate && booking.EndBookingDate <= b.EndBookingDate) ||
                                (booking.StartBookingDate <= b.StartBookingDate && booking.EndBookingDate >= b.EndBookingDate)));

            if (isVenueOverlapping)
            {
                ModelState.AddModelError("", "This venue already has an overlapping booking for this event.");
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

        // GET: Bookings/Filter
        public async Task<IActionResult> Filter()
        {
            var allBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();

            return View(allBookings);
        }


        public async Task<IActionResult> FilterResults(string searchTerm)
        {
            var bookingsQuery = _context.Bookings.Include(b => b.Event).Include(b => b.Venue).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingId.ToString().Contains(searchTerm)
                                                      || b.Event.EventName.Contains(searchTerm)
                                                      || b.Venue.VenueName.Contains(searchTerm));
            }

            var filteredBookings = await bookingsQuery.ToListAsync();
            return View("Filter", filteredBookings);
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
