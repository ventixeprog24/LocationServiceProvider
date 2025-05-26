using Grpc.Core;
using LocationServiceProvider.Interfaces;

namespace LocationServiceProvider.Services
{
    public class LocationGrpcService(ILocationService service) : LocationServiceContract.LocationServiceContractBase
    {
        private readonly ILocationService _service = service;

        public override Task<LocationReply> CreateLocation(LocationCreateRequest request, ServerCallContext context)
        {
            return _service.CreateLocationAsync(request);
        }

        public override Task<LocationByIdReply> GetLocationById(LocationByIdRequest request, ServerCallContext context)
        {
            return _service.GetLocationByIdAsync(request);
        }

        public override Task<LocationListReply> GetAllLocations(Empty request, ServerCallContext context)
        {
            return _service.GetAllLocationsAsync(request);
        }

        public override Task<LocationReply> UpdateLocation(LocationUpdateRequest request, ServerCallContext context)
        {
            return _service.UpdateLocationAsync(request);
        }

        public override Task<LocationReply> DeleteLocation(LocationByIdRequest request, ServerCallContext context)
        {
            return _service.DeleteLocationAsync(request);
        }
    }
}
