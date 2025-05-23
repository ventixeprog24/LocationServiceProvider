using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Models;

namespace LocationServiceProvider.Helpers
{
    public class LocationSeatValidator : ISeatValidator
    {
        public ValidationResult Validate(int seatCount, int rowCount, int gateCount)
        {
            if (seatCount == 0 && (rowCount > 0 || gateCount > 0))
                return ValidationResult.Failed("Rows and Gates cannot have values when no seats are provided.");

            if (seatCount > 0)
            {
                if (rowCount <= 0 || gateCount <= 0)
                    return ValidationResult.Failed("Rows or Gates must be greater than 0 when seats are provided.");

                if (rowCount >= seatCount)
                    return ValidationResult.Failed("Rows must be less than the number of seats.");

                if (gateCount >= seatCount)
                    return ValidationResult.Failed("Gates must be less than the number of seats.");
            }

            return ValidationResult.Success();
        }
    }
}
