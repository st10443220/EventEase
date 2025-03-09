using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }
        public string? VenueName { get; set; }
        public string? VenueLocation { get; set; }
        public int VenueCapacity { get; set; }
        public string? VenueImageUrl { get; set; }

        // One Venue can have many Bookings
        public List<Booking> Bookings { get; set; } = new List<Booking>(); // Collection navigation property for the related events
    }
}
