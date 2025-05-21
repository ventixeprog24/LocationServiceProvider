using LocationServiceProvider.Data.Entities;

namespace LocationServiceProvider.Factories
{
    public class LocationFactory
    {
        public static LocationEntity? ToEntity(LocationCreateRequest request, List<LocationSeatCreate> seats)
        {
            seats ??= [];

            return new LocationEntity
            {
                Name = request.Name,
                StreetName = request.StreetName,
                PostalCode = request.PostalCode,
                City = request.City,
                Seats = seats.Select(seat => new LocationSeatEntity
                {
                    SeatNumber = seat.SeatNumber,
                    Row = seat.Row,
                    Gate = seat.Gate
                }).ToList() 
            };
        }

        public static bool UpdateEntity(LocationEntity entity, LocationUpdateRequest request, List<LocationSeatCreate> seats)
        {
            if (entity.Name != request.Name)
                entity.Name = request.Name;

            if (entity.StreetName != request.StreetName)
                entity.StreetName = request.StreetName;

            if (entity.PostalCode != request.PostalCode)
                entity.PostalCode = request.PostalCode;

            if (entity.City != request.City)
                entity.City = request.City;

            if (request.SeatCount > 0 && request.RowCount > 0)
            {
                entity.Seats.Clear();
                foreach (var seat in seats)
                {
                    entity.Seats.Add(new LocationSeatEntity
                    {
                        SeatNumber = seat.SeatNumber,
                        Row = seat.Row,
                        Gate = seat.Gate
                    });
                }
            }

            return true;
        }

        public static Location ToGrpcModel(LocationEntity entity)
        {
            var seats = entity.Seats.Select(seat => new LocationSeat
            {
                Id = seat.SeatId,
                SeatNumber = seat.SeatNumber,
                Row = seat.Row,
                Gate = seat.Gate
            }).ToList();

            var location = new Location
            {
                Id = entity.Id,
                Name = entity.Name,
                StreetName = entity.StreetName,
                PostalCode = entity.PostalCode,
                City = entity.City
            };

            location.Seats.AddRange(seats);

            return location;
        }
    }
}
