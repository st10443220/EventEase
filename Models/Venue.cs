using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }
        [Required]
        public string? VenueName { get; set; }
        [Required]
        public string? VenueLocation { get; set; }
        [Required]
        public int VenueCapacity { get; set; }
        public string? VenueImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }

        // One Venue can have many Bookings
        public List<Booking> Bookings { get; set; } = new List<Booking>(); // Collection navigation property for the related events
    }
}
