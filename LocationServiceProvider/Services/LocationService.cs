using Grpc.Core;
using LocationServiceProvider.Data.Repositories;
using LocationServiceProvider.Factories;
using LocationServiceProvider.Interfaces;
using System.Diagnostics;

namespace LocationServiceProvider.Services
{
    public class LocationService(ILocationRepository locationRepository, IRequiredFieldsValidator requestValidator, ICacheHandler cacheHandler, ISeatGenerator seatGenerator) : LocationServiceContract.LocationServiceContractBase
    {
        private readonly ILocationRepository _locationRepository = locationRepository;
        private readonly IRequiredFieldsValidator _requestValidator = requestValidator;
        private readonly ICacheHandler _cacheHandler = cacheHandler;
        private readonly ISeatGenerator _seatGenerator = seatGenerator;

        private const string _cacheKey = "Locations";

        public override async Task<LocationReply> CreateLocation(LocationCreateRequest request, ServerCallContext context)
        {
            var validation = _requestValidator.ValidateRequiredFields(request);
            if (!validation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = validation.ErrorMessage };

            try
            {
                if (await _locationRepository.ExistsAsync(x => x.Name == request!.Name))
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists" };

                var seats = new List<LocationSeatCreate>();
                seats = _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount);

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

        public async override Task<LocationByIdReply> GetLocationById(LocationByIdRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
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

        public async override Task<LocationReply> UpdateLocation(LocationUpdateRequest request, ServerCallContext context)
        {
            var validation = _requestValidator.ValidateRequiredFields(request);
            if (!validation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = validation.ErrorMessage };

            try
            {
                var locationNameExists = await _locationRepository.ExistsAsync(x => x.Name == request!.Name && x.Id != request.Id);
                if (locationNameExists)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists" };

                var entity = await _locationRepository.GetAsync(x => x.Id == request!.Id, i => i.Seats);
                if (entity == null)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location not found" };

                var seats = new List<LocationSeatCreate>();
                seats = _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount);

                var updatedEntity = LocationFactory.UpdateEntity(entity, request!, seats);
                if (!updatedEntity)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Failed to update location" };

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

        public async override Task<LocationReply> DeleteLocation(LocationByIdRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
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
