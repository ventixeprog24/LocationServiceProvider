using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LocationServiceProvider.Data.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class LocationEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string StreetName { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string City { get; set; } = null!;
        public virtual ICollection<LocationSeatEntity> Seats { get; set; } = [];
    }
}
