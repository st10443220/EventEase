using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        [Required]
        public string? EventName { get; set; }
        [Required]
        public DateOnly? StartEventDate { get; set; }
        [Required]
        public DateOnly? FinishEventDate { get; set; }
        [Required]
        public string? EventDescription { get; set; }

        // One Event can have many Bookings
        public List<Booking> Bookings { get; set; } = new List<Booking>(); // Collection navigation property for the related events
    }
}
