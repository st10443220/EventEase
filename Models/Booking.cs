using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        public DateOnly StartBookingDate { get; set; }
        public DateOnly EndBookingDate { get; set; }


        // Foreign key to Event (many-to-one relationship with Event)
        public int EventId { get; set; } // Foreign key to Event
        public Event? Event { get; set; } // Navigation property to Event

        // Foreign key to Venue (many-to-one relationship with Venue)
        public int VenueId { get; set; } // Foreign key to Venue
        public Venue? Venue { get; set; } // Navigation property to Venue
    }
}
