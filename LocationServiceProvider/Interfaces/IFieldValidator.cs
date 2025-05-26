using LocationServiceProvider.Models;

namespace LocationServiceProvider.Interfaces
{
    public interface IFieldValidator
    {
        ValidationResult Validate<T>(T model);
        ValidationResult Validate(string value, string fieldName);
    }
}
