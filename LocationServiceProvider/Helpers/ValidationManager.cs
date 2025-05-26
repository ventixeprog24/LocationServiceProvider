using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Models;

namespace LocationServiceProvider.Helpers
{
    public class ValidationManager(IFieldValidator fieldsValidator, ISeatValidator seatsValidator) : IValidationManager
    {
        private readonly IFieldValidator _fields = fieldsValidator;
        private readonly ISeatValidator _seats = seatsValidator;

        public ValidationResult ValidateCreateRequest(LocationCreateRequest request)
        {
            var fieldValidation = _fields.Validate(request);
            if (!fieldValidation.IsValid)
                return fieldValidation;

            var seatsValidation = _seats.Validate(request.SeatCount, request.RowCount, request.GateCount);
            if (!seatsValidation.IsValid)
                return seatsValidation;

            return ValidationResult.Success();
        }

        public ValidationResult ValidateUpdateRequest(LocationUpdateRequest request)
        {
            var fieldValidation = _fields.Validate(request);
            if (!fieldValidation.IsValid)
                return fieldValidation;

            var seatsValidation = _seats.Validate(request.SeatCount, request.RowCount, request.GateCount);
            if (!seatsValidation.IsValid)
                return seatsValidation;

            return ValidationResult.Success();
        }

        public ValidationResult ValidateRequestId(string id, string fieldName)
        {
            var requestId = _fields.Validate(id, fieldName);
            if (!requestId.IsValid)
                return requestId;

            return ValidationResult.Success();
        }
    }
}
