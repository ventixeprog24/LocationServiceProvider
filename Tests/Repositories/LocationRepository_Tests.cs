using LocationServiceProvider.Data.Contexts;
using LocationServiceProvider.Data.Entities;
using LocationServiceProvider.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Repositories
{
    public class LocationRepository_Tests : IDisposable
    {
        // Tester genererade till stor del med hjälp av AI

        private readonly ServiceProvider _provider;
        private readonly DataContext _context;
        private readonly ILocationRepository _repository;

        public LocationRepository_Tests()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase($"Testing_{Guid.NewGuid()}"));
            services.AddScoped<ILocationRepository, LocationRepository>();

            _provider = services.BuildServiceProvider();
            _context = _provider.GetRequiredService<DataContext>();
            _repository = _provider.GetRequiredService<ILocationRepository>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _provider.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTrue_WhenEntityIsCreated()
        {
            // Arrange
            var entity = new LocationEntity
            {
                Name = "Test name",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };

            // Act
            var result = await _repository.CreateAsync(entity);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEntityWithSeats_WhenIdIsValid()
        {
            // Arrange
            var location = new LocationEntity
            {
                Id = "loc-123",
                Name = "Test name",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location));

            // Act
            var result = await _repository.GetAsync(
                   x => x.Id == location.Id,
                   x => x.Seats);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test name", result!.Name);
            Assert.Equal(2, result.Seats.Count);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenIdDoesNotMatchAnyEntity()
        {
            var result = await _repository.GetAsync(
                x => x.Id == "non-existent-id",
                x => x.Seats);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldIncludeSeats_WhenIncludeExpressionIsGiven()
        {
            // Arrange
            var location = new LocationEntity
            {
                Id = "loc-123",
                Name = "TestName",
                StreetName = "Test street",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location));

            // Act
            var result = await _repository.GetAllAsync(includes: [x => x.Seats]);

            // Assert
            var loaded = result.FirstOrDefault(x => x.Id == "loc-123");
            Assert.NotNull(loaded);
            Assert.NotEmpty(loaded!.Seats);
            Assert.Equal(2, loaded.Seats.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLocations_WhenNoIncludesAreProvided()
        {
            // Arrange
            var location1 = new LocationEntity
            {
                Id = "loc-123",
                Name = "NameOne",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity1",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };

            var location2 = new LocationEntity
            {
                Id = "loc-321",
                Name = "NameTwo",
                StreetName = "Test street 2",
                PostalCode = "4321",
                City = "TestCity2",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location1));
            Assert.True(await _repository.CreateAsync(location2));

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenLocationExists()
        {
            // Arrange
            var location = new LocationEntity
            {
                Id = "loc-123",
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location));

            // Act
            var exists = await _repository.ExistsAsync(l => l.Id == location.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenLocationDoesNotExist()
        {
            var exists = await _repository.ExistsAsync(l => l.Id == "non-existent");

            Assert.False(exists);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnTrueAndUpdateFields()
        {
            // Arrange
            var location = new LocationEntity
            {
                Id = "loc-123",
                Name = "OldName",
                StreetName = "Old street 1",
                PostalCode = "1234",
                City = "OldCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location));

            var beforeUpdate = await _repository.GetAsync(x => x.Id == "loc-123", x => x.Seats);
            Assert.Equal("OldName", beforeUpdate!.Name);
            Assert.Equal("1", beforeUpdate.Seats.First().SeatNumber);

            location.Name = "Updated";
            location.Seats.First().SeatNumber = "10";

            // Act
            var result = await _repository.UpdateAsync(location);
            var updated = await _repository.GetAsync(x => x.Id == "loc-123", x => x.Seats);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated", updated!.Name);
            Assert.Equal("10", updated.Seats.First().SeatNumber);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrueAndRemoveEntity_WhenMatchingEntityExists()
        {
            // Arrange
            var location = new LocationEntity
            {
                Id = "loc-123",
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" },
                    new() { SeatNumber = "2", Row = "B", Gate = "1" }
                }
            };
            Assert.True(await _repository.CreateAsync(location));

            // Act
            var deleted = await _repository.DeleteAsync(l => l.Id == "loc-123");
            var exists = await _repository.GetAsync(x => x.Id == "loc-123");

            // Assert
            Assert.True(deleted);
            Assert.Null(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNonExistingEntity()
        {
            var result = await _repository.DeleteAsync(x => x.Name == "DoesNotExist");

            Assert.False(result);
        }
    }
}
