using LocationServiceProvider;
using LocationServiceProvider.Data.Contexts;
using LocationServiceProvider.Data.Repositories;
using LocationServiceProvider.Helpers;
using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Services
{
    // Tester genererade till stor del med hjälp av AI

    public class LocationService_Tests : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly DataContext _context;
        private readonly ILocationService _service;

        public LocationService_Tests()
        {
            var services = new ServiceCollection();
            services.AddDbContext<DataContext>(x =>
                x.UseInMemoryDatabase($"Testing_{Guid.NewGuid()}"));

            services.AddMemoryCache();
            services.AddTransient<IFieldValidator, RequiredFieldsValidator>();
            services.AddTransient<ISeatValidator, LocationSeatValidator>();
            services.AddTransient<ISeatGenerator, SeatGenerator>();
            services.AddSingleton<ICacheHandler, CacheHandler>();
            services.AddTransient<IValidationManager, ValidationManager>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ILocationService, LocationService>();

            _provider = services.BuildServiceProvider();
            _context = _provider.GetRequiredService<DataContext>();
            _service = _provider.GetRequiredService<ILocationService>();
        }

        private LocationCreateRequest CreateLocationRequest(
          string? name = null,
          string? street = null,
          string? postalCode = null,
          string? city = null,
          int seatCount = 20,
          int rowCount = 4,
          int gateCount = 2)
        {
            return new LocationCreateRequest
            {
                Name = name ?? "Test Arena",
                StreetName = street ?? "Test Street",
                PostalCode = postalCode ?? "12345",
                City = city ?? "TestCity",
                SeatCount = seatCount,
                RowCount = rowCount,
                GateCount = gateCount
            };
        }

        private LocationUpdateRequest CreateLocationUpdateRequest(
          string id,
          string? name = null,
          string? street = null,
          string? postalCode = null,
          string? city = null,
          int seatCount = 20,
          int rowCount = 4,
          int gateCount = 2)
        {
            return new LocationUpdateRequest
            {
                Id = id,
                Name = name ?? "Updated Arena",
                StreetName = street ?? "Updated Street",
                PostalCode = postalCode ?? "99999",
                City = city ?? "UpdatedCity",
                SeatCount = seatCount,
                RowCount = rowCount,
                GateCount = gateCount
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _provider.Dispose();
        }

        [Fact]
        public async Task CreateLocationAsync_ShouldSucceed_WhenInputIsValid()
        {
            // Arrange
            var request = CreateLocationRequest();

            // Act
            var created = await _service.CreateLocationAsync(request);

            // Assert
            Assert.True(created.Succeeded);

            var locations = await _service.GetAllLocationsAsync(new());
            Assert.Contains(locations.Locations, l => l.Name == request.Name);
        }

        [Theory]
        [InlineData("", "Test Street", "12345", "City")]
        [InlineData("ValidName", "", "12345", "City")]
        [InlineData("ValidName", "Test Street", "", "City")]
        [InlineData("ValidName", "Test Street", "12345", "")]
        public async Task CreateLocationAsync_ShouldFail_WithMissingRequiredFields(
                string name, string street, string postalCode, string city)
        {
            // Arrange
            var request = CreateLocationRequest(name, street, postalCode, city);

            // Act
            var result = await _service.CreateLocationAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));

            var all = await _service.GetAllLocationsAsync(new());
            Assert.Empty(all.Locations);
        }

        [Fact]
        public async Task CreateLocationAsync_ShouldSucceed_WhenSeatCountIsZero()
        {
            var request = CreateLocationRequest(seatCount: 0, rowCount: 0, gateCount: 0, name: "NoSeats Arena");

            var result = await _service.CreateLocationAsync(request);

            Assert.True(result.Succeeded);

            var location = (await _service.GetAllLocationsAsync(new())).Locations.FirstOrDefault(l => l.Name == request.Name);
            Assert.NotNull(location);
            Assert.Empty(location.Seats);
        }

        [Fact]
        public async Task CreateLocationAsync_ShouldFail_WhenNameExists()
        {
            // Arrange
            var request = CreateLocationRequest(name: "Duplicate Arena");

            var created = await _service.CreateLocationAsync(request);
            Assert.True(created.Succeeded);

            // Act
            var duplicateResult = await _service.CreateLocationAsync(request);

            // Assert
            Assert.False(duplicateResult.Succeeded);
            Assert.Equal("Location name already exists.", duplicateResult.ErrorMessage);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ShouldReturnCorrectLocation_WhenExists()
        {
            // Arrange
            var create = await _service.CreateLocationAsync(CreateLocationRequest());
            Assert.True(create.Succeeded);

            var all = await _service.GetAllLocationsAsync(new());
            var id = all.Locations.First().Id;

            // Act
            var result = await _service.GetLocationByIdAsync(new LocationByIdRequest { Id = id });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal("Test Arena", result.Location.Name);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ShouldFail_WhenLocationNotExists()
        {
            var result = await _service.GetLocationByIdAsync(new LocationByIdRequest { Id = "non-existent-id" });

            Assert.False(result.Succeeded);
            Assert.Equal("The location could not be found.", result.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetLocationByIdAsync_ShouldFail_WhenIdIsNullOrWhitespace(string invalidId)
        {
            // Arrange
            var request = new LocationByIdRequest { Id = invalidId };

            // Act
            var result = await _service.GetLocationByIdAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("ID is required.", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAllLocationsAsync_ShouldReturnAllLocations_WhenLocationsExist()
        {
            // Arrange
            var create1 = await _service.CreateLocationAsync(CreateLocationRequest(
                name: "Test Name 1", street: "Test Street 1", postalCode: "12345", city: "TestCity1", seatCount: 30, rowCount: 6, gateCount: 3));
            var create2 = await _service.CreateLocationAsync(CreateLocationRequest(
                name: "Test Name 2", street: "Test Street 2", postalCode: "54321", city: "TestCity2", seatCount: 20, rowCount: 4, gateCount: 2));

            Assert.True(create1.Succeeded);
            Assert.True(create2.Succeeded);

            // Act
            var result = await _service.GetAllLocationsAsync(new Empty());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(2, result.Locations.Count);
            Assert.Contains(result.Locations, l => l.Name == "Test Name 1" && l.City == "TestCity1");
            Assert.Contains(result.Locations, l => l.Name == "Test Name 2" && l.City == "TestCity2");
        }

        [Fact]
        public async Task UpdateLocationAsync_ShouldSucceed_WhenValidRequest()
        {
            // Arrange
            var create = await _service.CreateLocationAsync(CreateLocationRequest());
            Assert.True(create.Succeeded);

            var id = (await _service.GetAllLocationsAsync(new())).Locations.First().Id;
            Assert.NotNull(id);

            // Act
            var update = await _service.UpdateLocationAsync(new LocationUpdateRequest
            {
                Id = id,
                Name = "Updated Test Arena",
                StreetName = "Updated Street",
                PostalCode = "11111",
                City = "UpdatedCity",
                SeatCount = 30,
                RowCount = 5,
                GateCount = 1,
            });

            // Assert
            Assert.True(update.Succeeded);

            var result = await _service.GetLocationByIdAsync(new LocationByIdRequest { Id = id });
            Assert.Equal("Updated Test Arena", result.Location.Name);
        }

        [Theory]
        [InlineData("", "123", "Street", "12345", "City", 10, 2, 1)]
        [InlineData("ValidName", "", "Street", "12345", "City", 10, 2, 1)]
        [InlineData("ValidName", "123", "", "12345", "City", 10, 2, 1)]
        [InlineData("ValidName", "123", "Street", "", "City", 10, 2, 1)]
        [InlineData("ValidName", "123", "Street", "12345", "", 10, 2, 1)]
        [InlineData("ValidName", "123", "Street", "12345", "City", 0, 2, 1)]
        [InlineData("ValidName", "123", "Street", "12345", "City", 10, 0, 1)]
        [InlineData("ValidName", "123", "Street", "12345", "City", 10, 2, 0)]
        public async Task UpdateLocationAsync_ShouldFail_WhenRequestIsInvalid(
            string name,
            string id,
            string street,
            string postalCode,
            string city,
            int seatCount,
            int rowCount,
            int gateCount)
        {
            // Arrange
            var request = CreateLocationUpdateRequest(id, name, street, postalCode, city, seatCount, rowCount, gateCount);

            // Act
            var result = await _service.UpdateLocationAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));

            var all = await _service.GetAllLocationsAsync(new());
            Assert.Empty(all.Locations);
        }

        [Fact]
        public async Task UpdateLocationAsync_ShouldFail_WhenNameAlreadyExists()
        {
            // Arrange
            await _service.CreateLocationAsync(CreateLocationRequest(
                name: "Test Arena 1", street: "Street 1", postalCode: "12345", city: "City1", seatCount: 20, rowCount: 4, gateCount: 2));
            await _service.CreateLocationAsync(CreateLocationRequest(
                name: "Test Arena 2", street: "Street 2", postalCode: "54321", city: "City2", seatCount: 30, rowCount: 5, gateCount: 3));

            var idToUpdate = (await _service.GetAllLocationsAsync(new())).Locations
                .First(l => l.Name == "Test Arena 2").Id;
            Assert.NotNull(idToUpdate);

            // Act
            var result = await _service.UpdateLocationAsync(new LocationUpdateRequest
            {
                Id = idToUpdate,
                Name = "Test Arena 1",
                StreetName = "Updated Street",
                PostalCode = "99999",
                City = "UpdatedCity",
                SeatCount = 25,
                RowCount = 5,
                GateCount = 2
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Location name already exists.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateLocationAsync_ShouldFail_WhenLocationDoesNotExist()
        {
            // Arrange
            var request = CreateLocationUpdateRequest("non-existent-id");

            // Act
            var result = await _service.UpdateLocationAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("The location could not be found.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteLocationAsync_ShouldSucceed_WhenLocationExists()
        {
            // Arrange
            var create = await _service.CreateLocationAsync(CreateLocationRequest());
            Assert.True(create.Succeeded);

            var id = (await _service.GetAllLocationsAsync(new())).Locations.First().Id;

            // Act
            var deleteResult = await _service.DeleteLocationAsync(new LocationByIdRequest { Id = id });

            // Assert
            Assert.True(deleteResult.Succeeded);

            var all = await _service.GetAllLocationsAsync(new());
            Assert.DoesNotContain(all.Locations, l => l.Id == id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteLocationAsync_ShouldFail_WhenIdIsNullOrWhitespace(string invalidId)
        {
            // Act
            var result = await _service.DeleteLocationAsync(new LocationByIdRequest { Id = invalidId });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("ID is required.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteLocationAsync_ShouldFail_WhenLocationDoesNotExist()
        {
            // Act
            var result = await _service.DeleteLocationAsync(new LocationByIdRequest { Id = "invalid-id" });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No location found with given ID.", result.ErrorMessage);
        }
    }
}
