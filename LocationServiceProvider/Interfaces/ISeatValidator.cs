using LocationServiceProvider.Models;

namespace LocationServiceProvider.Interfaces
{
    public interface ISeatValidator 
    {
        ValidationResult Validate(int seatCount, int rowCount, int gateCount);
    }
}
