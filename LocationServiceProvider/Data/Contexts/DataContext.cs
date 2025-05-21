using LocationServiceProvider.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocationServiceProvider.Data.Contexts
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<LocationEntity> Locations { get; set; }
        public DbSet<LocationSeatEntity> LocationSeats { get; set; }
    }
}
