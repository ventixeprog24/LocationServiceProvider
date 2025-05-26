using LocationServiceProvider;
using LocationServiceProvider.Data.Entities;
using LocationServiceProvider.Factories;

namespace Tests.Factories
{
    public class LocationFactory_Tests
    {
        // Tester genererade till stor del med hjälp av AI

        [Fact]
        public void ToEntity_ShouldCreateEntityAndMapCorrectly_WhenInputIsValid()
        {
            // Arrange
            var request = new LocationCreateRequest
            {
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity"
            };

            var seats = new List<LocationSeatCreate>
            {
                new() { SeatNumber = "1", Row = "A", Gate = "1" },
                new() { SeatNumber = "2", Row = "A", Gate = "1" }
            };

            // Act
            var entity = LocationFactory.ToEntity(request, seats);

            // Assert
            Assert.Equal(request.Name, entity!.Name);
            Assert.Equal(request.StreetName, entity.StreetName);
            Assert.Equal(request.PostalCode, entity.PostalCode);
            Assert.Equal(request.City, entity.City);
            Assert.Equal(2, entity.Seats.Count);
            Assert.All(seats, seat =>
                Assert.Contains(entity.Seats, s =>
                    s.SeatNumber == seat.SeatNumber &&
                    s.Row == seat.Row &&
                    s.Gate == seat.Gate));
        }

        [Fact]
        public void ToEntity_ShouldMapAndCreateEntityWithEmptySeats_WhenSeatsAreEmpty()
        {
            // Arrange
            var request = new LocationCreateRequest
            {
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity"
            };
            var seats = new List<LocationSeatCreate>();

            // Act
            var entity = LocationFactory.ToEntity(request, seats);

            // Assert
            Assert.Equal(request.Name, entity!.Name);
            Assert.Equal(request.StreetName, entity.StreetName);
            Assert.Equal(request.PostalCode, entity.PostalCode);
            Assert.Equal(request.City, entity.City);
            Assert.NotNull(entity);
            Assert.NotNull(entity.Seats);
            Assert.Empty(entity.Seats);
        }

        [Fact]
        public void UpdateEntity_ShouldUpdateFieldsAndSeats_WhenValuesAreDifferent()
        {
            // Arrange
            var entity = new LocationEntity
            {
                Name = "OldName",
                StreetName = "Old Street",
                PostalCode = "11111",
                City = "OldCity",
                Seats = new List<LocationSeatEntity>
                {
                    new() { SeatNumber = "1", Row = "A", Gate = "1" }
                }
            };

            var request = new LocationUpdateRequest
            {
                Name = "NewName",
                StreetName = "New Street",
                PostalCode = "22222",
                City = "NewCity",
                SeatCount = 2,
                RowCount = 1,
                GateCount = 1
            };

            var newSeats = new List<LocationSeatCreate>
            {
                new () { SeatNumber = "2", Row = "B", Gate = "2" },
                new () { SeatNumber = "3", Row = "B", Gate = "2" }
            };

            // Act
            LocationFactory.UpdateEntity(entity, request, newSeats);

            // Assert
            Assert.Equal("NewName", entity.Name);
            Assert.Equal("New Street", entity.StreetName);
            Assert.Equal("22222", entity.PostalCode);
            Assert.Equal("NewCity", entity.City);
            Assert.Equal(2, entity.Seats.Count);
            Assert.Contains(entity.Seats, s => s.SeatNumber == "2" && s.Row == "B");
            Assert.Contains(entity.Seats, s => s.SeatNumber == "3" && s.Row == "B");
        }

        [Theory]
        [InlineData(0, 1, 1)] 
        [InlineData(1, 0, 1)] 
        [InlineData(0, 0, 1)] 
        public void UpdateEntity_ShouldNotUpdateSeats_WhenCountsInvalidOrSeatsEmpty(int seatCount, int rowCount, int gateCount)
        {
            // Arrange
            var originalSeat = new LocationSeatEntity { SeatNumber = "1", Row = "A", Gate = "1", LocationId = "L1", Location = null! };
            var entity = new LocationEntity
            {
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity> { originalSeat }
            };

            var request = new LocationUpdateRequest
            {
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                SeatCount = seatCount,
                RowCount = rowCount,
                GateCount = gateCount
            };

            var seats = new List<LocationSeatCreate>(); 

            // Act
            var exception = Record.Exception(() => LocationFactory.UpdateEntity(entity, request, seats));

            // Assert
            Assert.Null(exception);
            Assert.Single(entity.Seats);
            Assert.Equal(originalSeat.SeatNumber, entity.Seats.First().SeatNumber);
        }

        [Fact]
        public void ToGrpcModel_ShouldCreateModelAndMapCorrectly_WhenInputIsValid()
        {
            // Arrange
            var entity = new LocationEntity
            {
                Id = "loc-123",
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>
                {
                    new () { SeatId = "seat-1", SeatNumber = "1", Row = "A", Gate = "1" },
                    new () { SeatId = "seat-2", SeatNumber = "2", Row = "A", Gate = "1" }
                }
            };

            // Act
            var grpcModel = LocationFactory.ToGrpcModel(entity);

            // Assert
            Assert.Equal(entity.Id, grpcModel!.Id);
            Assert.Equal(entity.Name, grpcModel.Name);
            Assert.Equal(entity.StreetName, grpcModel.StreetName);
            Assert.Equal(entity.PostalCode, grpcModel.PostalCode);
            Assert.Equal(entity.City, grpcModel.City);
            Assert.Equal(entity.Seats.Count, grpcModel.Seats.Count);
            foreach (var seat in entity.Seats)
            {
                Assert.Contains(grpcModel.Seats, s =>
                    s.Id == seat.SeatId &&
                    s.SeatNumber == seat.SeatNumber &&
                    s.Row == seat.Row &&
                    s.Gate == seat.Gate);
            }
        }

        [Fact]
        public void ToGrpcModel_ShouldCreateModelWithEmptySeats_WhenSeatListIsEmpty()
        {
            // Arrange
            var entity = new LocationEntity
            {
                Id = "loc-123",
                Name = "TestName",
                StreetName = "Test street 1",
                PostalCode = "1234",
                City = "TestCity",
                Seats = new List<LocationSeatEntity>()
            };

            // Act
            var grpcModel = LocationFactory.ToGrpcModel(entity);

            // Assert
            Assert.NotNull(grpcModel);
            Assert.Equal("loc-123", grpcModel.Id);
            Assert.Empty(grpcModel.Seats);
        }
    }
}
