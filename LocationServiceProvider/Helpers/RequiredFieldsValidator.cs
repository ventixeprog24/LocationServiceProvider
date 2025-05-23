using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Models;

namespace LocationServiceProvider.Helpers
{
    public class RequiredFieldsValidator : IFieldValidator
    {
        public ValidationResult Validate<T>(T model)
        {
            if (model == null)
                return ValidationResult.Failed("Required fields are missing.");

            foreach (var property in typeof(T).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(model) as string;
                    if (string.IsNullOrWhiteSpace(value))
                        return ValidationResult.Failed("One or more required fields are missing.");
                }
                else if (property.PropertyType == typeof(int))
                {
                    var value = (int)property.GetValue(model)!;
                    if (value < 0)
                        return ValidationResult.Failed($"'{property.Name}' cannot be a negative value.");
                }
            }
            return ValidationResult.Success();
        }
    }
}
