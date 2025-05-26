using LocationServiceProvider.Helpers;

namespace Tests.Helpers
{
    public class LocationSeatValidator_Tests
    {
        // Tester genererade till stor del med hjälp av AI

        private readonly LocationSeatValidator _seatValidator = new();

        [Fact]
        public void Validate_ShouldReturnSuccess_WhenAllCountsAreZero()
        {
            var result = _seatValidator.Validate(0, 0, 0);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(10, 5, 5)]
        [InlineData(3, 1, 2)]
        [InlineData(2, 1, 1)]
        public void Validate_ShouldReturnSuccess_WhenSeatRowAndGateCountsAreValid(int seatCount, int rowCount, int gateCount)
        {
            var result = _seatValidator.Validate(seatCount, rowCount, gateCount);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(5, 0, 2)]
        [InlineData(5, 2, 0)]
        [InlineData(5, -1, 1)]
        [InlineData(5, 1, -2)]
        [InlineData(5, 0, 0)]
        public void Validate_ShouldReturnFailed_WhenSeatsProvidedButRowOrGateCountIsZeroOrNegative(int seatCount, int rowCount, int gateCount)
        {
            var result = _seatValidator.Validate(seatCount, rowCount, gateCount);

            Assert.False(result.IsValid);
            Assert.Equal("Rows or Gates must be greater than 0 when seats are provided.", result.ErrorMessage);
        }

        [Theory]
        [InlineData(5, 5, 3)]
        [InlineData(5, 3, 5)]
        [InlineData(5, 5, 5)]
        [InlineData(5, 6, 4)]
        [InlineData(5, 2, 6)]
        public void Validate_ShouldReturnFailed_WhenRowOrGateCountIsNotLessThanSeatCount(int seatCount, int rowCount, int gateCount)
        {
            var result = _seatValidator.Validate(seatCount, rowCount, gateCount);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(0, 1, 0)]
        [InlineData(0, 0, 1)]
        [InlineData(0, 2, 2)]
        [InlineData(0, -1, 1)]
        [InlineData(0, 1, -1)]
        public void Validate_ShouldReturnFailed_WhenSeatCountIsZeroButRowOrGateCountIsNonZero(int seatCount, int rowCount, int gateCount)
        {
            var result = _seatValidator.Validate(seatCount, rowCount, gateCount);

            Assert.False(result.IsValid);
            Assert.Equal("Rows and Gates cannot have values when no seats are provided.", result.ErrorMessage);
        }
    }
}
