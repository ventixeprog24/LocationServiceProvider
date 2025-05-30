﻿using LocationServiceProvider.Data.Repositories;
using LocationServiceProvider.Factories;
using LocationServiceProvider.Interfaces;
using System.Diagnostics;

namespace LocationServiceProvider.Services
{
    public class LocationService(ILocationRepository locationRepository, ICacheHandler cacheHandler, ISeatGenerator seatGenerator, IValidationManager validation) : ILocationService
    {
        private readonly ILocationRepository _locationRepository = locationRepository;
        private readonly ICacheHandler _cacheHandler = cacheHandler;
        private readonly ISeatGenerator _seatGenerator = seatGenerator;
        private readonly IValidationManager _validation = validation;

        private const string _cacheKey = "Locations";

        public async Task<LocationReply> CreateLocationAsync(LocationCreateRequest request)
        {
            var validation = _validation.ValidateCreateRequest(request);
            if (!validation.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = validation.ErrorMessage };

            try
            {
                if (await _locationRepository.ExistsAsync(x => x.Name == request!.Name))
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists." };

                var seats = request.SeatCount > 0
                    ? _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount)
                    : new List<LocationSeatCreate>();

                var entity = LocationFactory.ToEntity(request!, seats);
                if (entity == null)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Failed to create location." };

                var created = await _locationRepository.CreateAsync(entity);
                if (!created)
                    return new LocationReply { Succeeded = false, ErrorMessage = "The location could not be saved." };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "An error occured while creating the location." };
            }
        }

        public async Task<LocationByIdReply> GetLocationByIdAsync(LocationByIdRequest request)
        {
            var requestId = _validation.ValidateRequestId(request.Id, "ID");
            if (!requestId.IsValid)
                return new LocationByIdReply { Succeeded = false, ErrorMessage = requestId.ErrorMessage };

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

                return new LocationByIdReply { Succeeded = false, ErrorMessage = "The location could not be found." };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationByIdReply { Succeeded = false, ErrorMessage = "An error occured while retriving the location." };
            }
        }

        public async Task<LocationListReply> GetAllLocationsAsync(Empty request)
        {
            try
            {
                var cached = _cacheHandler.GetFromCache(_cacheKey) ?? await RefreshCacheAsync();
                return new LocationListReply { Succeeded = true, Locations = { cached } };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationListReply { Succeeded = false, ErrorMessage = "An error occured while loading the locations." };
            }
        }

        public async Task<LocationReply> UpdateLocationAsync(LocationUpdateRequest request)
        {
            var requestId = _validation.ValidateRequestId(request.Id, "ID");
            if (!requestId.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = requestId.ErrorMessage };

            try
            {
                var entity = await _locationRepository.GetAsync(x => x.Id == request!.Id, i => i.Seats);
                if (entity == null)
                    return new LocationReply { Succeeded = false, ErrorMessage = "The location could not be found." };

                var validation = _validation.ValidateUpdateRequest(request);
                if (!validation.IsValid)
                    return new LocationReply { Succeeded = false, ErrorMessage = validation.ErrorMessage };

                var locationNameExists = await _locationRepository.ExistsAsync(x => x.Name == request!.Name && x.Id != request.Id);
                if (locationNameExists)
                    return new LocationReply { Succeeded = false, ErrorMessage = "Location name already exists." };

                var seats = request.SeatCount > 0
                    ? _seatGenerator.GenerateSeats(request.SeatCount, request.RowCount, request.GateCount)
                    : new List<LocationSeatCreate>();

                LocationFactory.UpdateEntity(entity, request!, seats);

                var updated = await _locationRepository.UpdateAsync(entity);
                if (!updated)
                    return new LocationReply { Succeeded = false, ErrorMessage = "The location could not be updated." };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "An error occured while trying to updating the location." };
            }
        }

        public async Task<LocationReply> DeleteLocationAsync(LocationByIdRequest request)
        {
            var requestId = _validation.ValidateRequestId(request.Id, "ID");
            if (!requestId.IsValid)
                return new LocationReply { Succeeded = false, ErrorMessage = requestId.ErrorMessage };

            try
            {
                var entity = await _locationRepository.ExistsAsync(x => x.Id == request.Id);
                if (!entity)
                    return new LocationReply { Succeeded = false, ErrorMessage = "No location found with given ID." };

                var deleted = await _locationRepository.DeleteAsync(x => x.Id == request.Id);
                if (!deleted)
                    return new LocationReply { Succeeded = false, ErrorMessage = "The location could not be deleted." };

                await RefreshCacheAsync();

                return new LocationReply { Succeeded = true };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new LocationReply { Succeeded = false, ErrorMessage = "An error occured while trying to delete the location." };
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

