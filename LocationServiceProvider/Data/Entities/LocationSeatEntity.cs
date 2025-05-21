using System.ComponentModel.DataAnnotations;

namespace LocationServiceProvider.Data.Entities
{
    public class LocationSeatEntity
    {
        [Key]
        public string SeatId { get; set; } = Guid.NewGuid().ToString();
        public string SeatNumber { get; set; } = null!;
        public string Row { get; set; } = null!;
        public string Gate { get; set; } = null!;
        public string LocationId { get; set; } = null!;
        public virtual LocationEntity Location { get; set; } = null!;
    }
}
