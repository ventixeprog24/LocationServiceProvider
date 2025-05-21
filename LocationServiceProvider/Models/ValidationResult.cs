namespace LocationServiceProvider.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failed(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
    }
}
