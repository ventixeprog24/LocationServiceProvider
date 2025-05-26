using LocationServiceProvider;
using LocationServiceProvider.Helpers;
using LocationServiceProvider.Interfaces;
using LocationServiceProvider.Models;
using Moq;

namespace Tests.Helpers
{
    public class ValidationManager_Tests
    {
        // Tester genererade till stor del med hjälp av AI

        private readonly Mock<IFieldValidator> _fieldValidatorMock;
        private readonly Mock<ISeatValidator> _seatValidatorMock;
        private readonly ValidationManager _validationManager;

        public ValidationManager_Tests()
        {
            _fieldValidatorMock = new Mock<IFieldValidator>();
            _seatValidatorMock = new Mock<ISeatValidator>();
            _validationManager = new ValidationManager(_fieldValidatorMock.Object, _seatValidatorMock.Object);
        }

        [Fact]
        public void ValidateCreateRequest_ShouldReturnFailed_WhenInvalidFields()
        {
            // Arrange
            var request = new LocationCreateRequest(); 
            var error = ValidationResult.Failed("Field invalid");
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(error);

            // Act
            var result = _validationManager.ValidateCreateRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Field invalid", result.ErrorMessage); 
            _fieldValidatorMock.Verify(v => v.Validate(request), Times.Once);
            _seatValidatorMock.Verify(v => v.Validate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ValidateCreateRequest_ShouldReturnFailed_WhenInvalidSeats()
        {
            // Arrange
            var request = new LocationCreateRequest { SeatCount = 10, RowCount = 2, GateCount = 1 };
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
            _seatValidatorMock.Setup(v => v.Validate(10, 2, 1)).Returns(ValidationResult.Failed("Seat invalid"));

            // Act
            var result = _validationManager.ValidateCreateRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Seat invalid", result.ErrorMessage);
        }

        [Fact]
        public void ValidateCreateRequest_ShouldReturnSuccess_WhenFieldsAndSeatsAreValid()
        {
            // Arrange
            var request = new LocationCreateRequest { SeatCount = 10, RowCount = 2, GateCount = 1 };
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
            _seatValidatorMock.Setup(v => v.Validate(10, 2, 1)).Returns(ValidationResult.Success());

            // Act
            var result = _validationManager.ValidateCreateRequest(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void ValidateRequestId_ShouldReturnFailed_WhenInvalidField()
        {
            // Arrange
            var error = ValidationResult.Failed("ID is invalid");
            _fieldValidatorMock.Setup(v => v.Validate("bad-id", "ID")).Returns(error);

            // Act
            var result = _validationManager.ValidateRequestId("bad-id", "ID");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("ID is invalid", result.ErrorMessage);
        }

        [Fact]
        public void ValidateRequestId_ShouldReturnSuccess_WhenValidField()
        {
            // Arrange
            _fieldValidatorMock.Setup(v => v.Validate("good-id", "ID")).Returns(ValidationResult.Success());

            // Act
            var result = _validationManager.ValidateRequestId("good-id", "ID");

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateUpdateRequest_ShouldReturnSuccess_WhenFieldsAndSeatsAreValid()
        {
            // Arrange
            var request = new LocationUpdateRequest { SeatCount = 5, RowCount = 1, GateCount = 1 };
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
            _seatValidatorMock.Setup(v => v.Validate(5, 1, 1)).Returns(ValidationResult.Success());

            // Act
            var result = _validationManager.ValidateUpdateRequest(request);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateUpdateRequest_ShouldReturnFailed_WhenInvalidFields()
        {
            // Arrange
            var request = new LocationUpdateRequest();
            var error = ValidationResult.Failed("Invalid update fields");
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(error);

            // Act
            var result = _validationManager.ValidateUpdateRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Invalid update fields", result.ErrorMessage);
            _seatValidatorMock.Verify(v => v.Validate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ValidateUpdateRequest_ShouldReturnFailed_WhenInvalidSeats()
        {
            // Arrange
            var request = new LocationUpdateRequest { SeatCount = 20, RowCount = 3, GateCount = 1 };
            _fieldValidatorMock.Setup(v => v.Validate(request)).Returns(ValidationResult.Success());
            _seatValidatorMock.Setup(v => v.Validate(20, 3, 1)).Returns(ValidationResult.Failed("Seat config invalid"));

            // Act
            var result = _validationManager.ValidateUpdateRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Seat config invalid", result.ErrorMessage);
        }
    }
}
