using LocationServiceProvider.Models;

namespace LocationServiceProvider.Interfaces
{
    public interface IRequiredFieldsValidator
    {
        ValidationResult ValidateRequiredFields<T>(T model);
    }
}
