using LocationServiceProvider.Models;

namespace LocationServiceProvider.Interfaces
{
    public interface IValidationManager
    {
        ValidationResult ValidateCreateRequest(LocationCreateRequest request);
        ValidationResult ValidateRequestId(string id, string fieldName);
        ValidationResult ValidateUpdateRequest(LocationUpdateRequest request);
    }
}
