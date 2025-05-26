using LocationServiceProvider;
using LocationServiceProvider.Helpers;

namespace Tests.Helpers
{
    public class RequiredFieldsValidator_Tests
    {
        // Tester genererade till stor del med hjälp av AI

        private readonly RequiredFieldsValidator _fieldValidator = new();

        [Fact]
        public void Validate_Model_ShouldReturnSuccess_WhenInputIsValid()
        {
            // Arrange
            var model = new LocationCreateRequest
            {
                Name = "TestName",
                StreetName = "Street name",
                PostalCode = "12345",
                City = "City",
                SeatCount = 10,
                RowCount = 1,
                GateCount = 1,
            };

            // Act
            var result = _fieldValidator.Validate(model);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_Model_ShouldReturnFailed_WhenModelIsNull()
        {
            // Arrange
            LocationCreateRequest? model = null;

            // Act
            var result = _fieldValidator.Validate(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Required fields are missing.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_Model_ShouldReturnFailed_WhenIntInputIsNegative()
        {
            // Arrange
            var model = new LocationCreateRequest
            {
                Name = "TestName",
                StreetName = "Street name",
                PostalCode = "12345",
                City = "City",
                SeatCount = 10,
                RowCount = 0,
                GateCount = -1,
            };

            // Act
            var result = _fieldValidator.Validate(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot be a negative value", result.ErrorMessage);
        }

        [Theory]
        [InlineData(null, "Street", "1234", "City")]
        [InlineData("Name", null, "1234", "City")]
        [InlineData("Name", "Street", null, "City")]
        [InlineData("Name", "Street", "1234", null)]
        [InlineData("", "Street", "1234", "City")]
        [InlineData("Name", "", "1234", "City")]
        [InlineData("Name", "Street", "", "City")]
        [InlineData("Name", "Street", "1234", "")]
        public void Validate_Model_ShouldReturnFailed_WhenRequiredStringsAreInvalid(string? name, string? streetName, string? postalCode, string? city)
        {
            // Arrange
            var model = new LocationCreateRequest
            {
                Name = name ?? "",
                StreetName = streetName ?? "",
                PostalCode = postalCode ?? "",
                City = city ?? ""
            };

            // Act
            var result = _fieldValidator.Validate(model);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("One or more required fields are missing.", result.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_String_ShouldReturnFailed_WhenValueIsNullOrWhitespace(string? invalidValue)
        {
            var result = _fieldValidator.Validate(invalidValue!, "Id");

            Assert.False(result.IsValid);
            Assert.Equal("Id is required.", result.ErrorMessage);
        }

        [Fact]
        public void Validate_String_ShouldReturnSuccess_WhenValueIsValid()
        {
            var result = _fieldValidator.Validate("validId", "Id");

            Assert.True(result.IsValid);
        }
    }
}
