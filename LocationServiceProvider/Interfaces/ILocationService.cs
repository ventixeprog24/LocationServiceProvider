namespace LocationServiceProvider.Interfaces
{
    public interface ILocationService
    {
        Task<LocationReply> CreateLocationAsync(LocationCreateRequest request);
        Task<LocationReply> DeleteLocationAsync(LocationByIdRequest request);
        Task<LocationListReply> GetAllLocationsAsync(Empty request);
        Task<LocationByIdReply> GetLocationByIdAsync(LocationByIdRequest request);
        Task<LocationReply> UpdateLocationAsync(LocationUpdateRequest request);
    }
}

