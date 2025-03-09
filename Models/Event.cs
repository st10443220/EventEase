using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateOnly? StartEventDate { get; set; }
        public DateOnly? FinishEventDate { get; set; }

        public string? EventDescription { get; set; }

        // One Event can have many Bookings
        public List<Booking> Bookings { get; set; } = new List<Booking>(); // Collection navigation property for the related events
    }
}
