using Grpc.Core;
using LocationServiceProvider.Data.Repositories;
using LocationServiceProvider.Factories;
using LocationServiceProvider.Interfaces;
using System.Diagnostics;

namespace LocationServiceProvider.Services
{
    public class LocationService(ILocationRepository locationRepository, IFieldValidator fieldsValidator, ISeatValidator seatsValidator, ICacheHandler cacheHandler, ISeatGenerator seatGenerator) : LocationServiceContract.LocationServiceContractBase
    {
        private readonly ILocationRepository _locationRepository = locationRepository;
        private readonly IFieldValidator _fieldsValidator = fieldsValidator;
        private readonly ISeatValidator _seatsValidator = seatsValidator;
        private readonly ICacheHandler _cacheHandler = cacheHandler;
        private readonly ISeatGenerator _seatGenerator = seatGenerator;

        private const string _cacheKey = "Locations";

        public override async Task<LocationReply> CreateLocation(LocationCreateRequest request, ServerCallContext context)
        {
            var fieldValidation = _fieldsValidator.Validate(request);
            if (!fieldValidation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = fieldValidation.ErrorMessage };

            var seatsValidation = _seatsValidator.Validate(request.SeatCount, request.RowCount, request.GateCount);
            if (!seatsValidation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = seatsValidation.ErrorMessage };

            try
            {
                if (await _locationRepository.ExistsAsync(x => x.Name == request!.Name))
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists" };

                var seats = request.SeatCount > 0
                    ? _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount)
                    : new List<LocationSeatCreate>();

                var entity = LocationFactory.ToEntity(request!, seats);
                if (entity == null)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Failed to create location" };

                var created = await _locationRepository.CreateAsync(entity);
                if (!created)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Failed to create location" };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "Unable to create location" };
            }
        }

        public override async Task<LocationByIdReply> GetLocationById(LocationByIdRequest request, ServerCallContext context)
        {
            var requestId = _fieldsValidator.Validate(request.Id);
            if (!requestId.IsValid)
                return new LocationByIdReply { Succeeded = false, ErrorMessage = "ID is required." };

            try
            {
                var cached = _cacheHandler.GetFromCache(_cacheKey);

                var match = cached?.FirstOrDefault(x => x.Id == request.Id);
                if (match != null)
                    return new LocationByIdReply { Succeeded = true, Location = match };

                var models = await RefreshCacheAsync();

                var model = models.FirstOrDefault(x => x.Id == request.Id);
                if (model != null)
                    return new LocationByIdReply { Succeeded = true, Location = model };

                return new LocationByIdReply { Succeeded = false, ErrorMessage = "Location not found." };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationByIdReply { Succeeded = false, ErrorMessage = "Unable to get location" };
            }
        }

        public override async Task<LocationListReply> GetAllLocations(Empty request, ServerCallContext context)
        {
            try
            {
                var cached = _cacheHandler.GetFromCache(_cacheKey) ?? await RefreshCacheAsync();
                return new LocationListReply { Succeeded = true, Locations = { cached } };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationListReply { Succeeded = false, ErrorMessage = "Unable to get locations" };
            }
        }

        public override async Task<LocationReply> UpdateLocation(LocationUpdateRequest request, ServerCallContext context)
        {
            var fieldValidation = _fieldsValidator.Validate(request);
            if (!fieldValidation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = fieldValidation.ErrorMessage };

            var seatsValidation = _seatsValidator.Validate(request.SeatCount, request.RowCount, request.GateCount);
            if (!seatsValidation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = seatsValidation.ErrorMessage };

            try
            {
                var locationNameExists = await _locationRepository.ExistsAsync(x => x.Name == request!.Name && x.Id != request.Id);
                if (locationNameExists)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists" };

                var entity = await _locationRepository.GetAsync(x => x.Id == request!.Id, i => i.Seats);
                if (entity == null)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location not found" };

                var seats = request.SeatCount > 0
                    ? _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount)
                    : new List<LocationSeatCreate>();

                LocationFactory.UpdateEntity(entity, request!, seats);

                var updated = await _locationRepository.UpdateAsync(entity);
                if (!updated)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Failed to update location" };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "Unable to update location" };
            }
        }

        public override async Task<LocationReply> DeleteLocation(LocationByIdRequest request, ServerCallContext context)
        {
            var requestId = _fieldsValidator.Validate(request.Id);
            if (!requestId.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = "ID is required." };

            try
            {
                var entity = await _locationRepository.ExistsAsync(x => x.Id == request.Id);
                if (!entity)
                    return new LocationReply { Succeeded = false, ErrorMessage = "No location found with given id" };

                var deleted = await _locationRepository.DeleteAsync(x => x.Id == request.Id);
                if (!deleted)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location deletion failed" };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "Unable to delete location" };
            }
        }

        public async Task<IEnumerable<Location>> RefreshCacheAsync()
        {
            var entities = await _locationRepository.GetAllAsync(
                sortBy: x => x.Name,
                includes: [x => x.Seats]
            );

            if (entities == null)
                return [];

            var models = entities.Select(LocationFactory.ToGrpcModel).ToList();

            _cacheHandler.SetCache(_cacheKey, models);

            return models;
        }
    }
}
