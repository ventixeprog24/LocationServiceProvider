using LocationServiceProvider.Helpers;

namespace Tests.Helpers
{
    public class SeatGenerator_Tests
    {
        // Tester genererade till stor del med hjälp av AI

        private readonly SeatGenerator _generator = new();

        [Theory]
        [InlineData(10, 2, 1)]
        [InlineData(15, 3, 2)]
        [InlineData(6, 2, 1)]
        public void GenerateSeats_ShouldReturnCorrectStructureAndCount_WhenInputIsValid(int seatCount, int rowCount, int gateCount)
        {
            // Act
            var result = _generator.GenerateSeats(seatCount, rowCount, gateCount);

            // Assert
            Assert.Equal(seatCount, result.Count);
            Assert.All(result, seat =>
            {
                Assert.False(string.IsNullOrWhiteSpace(seat.SeatNumber));
                Assert.False(string.IsNullOrWhiteSpace(seat.Row));
                Assert.False(string.IsNullOrWhiteSpace(seat.Gate));
            });
        }

        [Fact]
        public void GenerateSeats_ShouldDistributeExtraSeatsEvenly_WhenExtraSeatsExists()
        {
            // Arrange
            int seatCount = 11;
            int rowCount = 3;
            int gateCount = 1;

            // Act
            var result = _generator.GenerateSeats(seatCount, rowCount, gateCount);

            // Assert
            Assert.Equal(11, result.Count);

            var rowA = result.Count(s => s.Row == "A");
            var rowB = result.Count(s => s.Row == "B");
            var rowC = result.Count(s => s.Row == "C");
            Assert.Equal(4, rowA);
            Assert.Equal(4, rowB);
            Assert.Equal(3, rowC);
        }

        [Fact] 
        public void GenerateSeats_ShouldGenerateValidMultiLetterRowLabels_WhenRowsAreMoreThan26()
        {
            // Arrange
            int rowCount = 130;
            int seatCount = 130;
            int gateCount = 2;

            var result = _generator.GenerateSeats(seatCount, rowCount, gateCount);

            // Assert
            Assert.Equal(seatCount, result.Count);
            Assert.All(result, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.Row));
                Assert.Matches("^[A-Z]+$", s.Row); 
            });
        }

        [Theory]
        [InlineData(5, 1, 5)]
        [InlineData(6, 2, 3)]
        [InlineData(7, 3, 3)]
        public void CalculateRowsPerGate_ShouldReturnExpectedRowsPerGate_WhenValidRowAndGateCount(int rowCount, int gateCount, int expected)
        {
            // Act
            var result = _generator.CalculateRowsPerGate(rowCount, gateCount);

            // Assert
            Assert.Equal(expected, result);
        }


        [Fact]
        public void CreateSeatsWithGates_ShouldReturnExpectedTotalSeatCount_WhenInputIsValid()
        {
            // Arrange
            int rowCount = 5;
            int seatsPerRow = 3;
            int extraSeats = 2;
            int rowsPerGate = 2;

            // Act
            var seats = _generator.CreateSeatsWithGates(rowCount, seatsPerRow, extraSeats, rowsPerGate);

            // Assert 
            Assert.Equal(17, seats.Count);
        }

        [Fact]
        public void CreateSeatsWithGates_ShouldDistributeExtraSeatsAndAssignCorrectGateLabels()
        {
            // Arrange
            int rowCount = 5;
            int seatsPerRow = 3;
            int extraSeats = 2;
            int rowsPerGate = 2;

            // Act
            var seats = _generator.CreateSeatsWithGates(rowCount, seatsPerRow, extraSeats, rowsPerGate);
            var seatsByRow = seats.GroupBy(s => s.Row).ToDictionary(g => g.Key, g => g.ToList());

            // Assert: seats per row
            Assert.Equal(4, seatsByRow["A"].Count);
            Assert.Equal(4, seatsByRow["B"].Count);
            Assert.Equal(3, seatsByRow["C"].Count);
            Assert.Equal(3, seatsByRow["D"].Count);
            Assert.Equal(3, seatsByRow["E"].Count);
            // Assert: gate per row
            Assert.All(seatsByRow["A"], s => Assert.Equal("1", s.Gate));
            Assert.All(seatsByRow["B"], s => Assert.Equal("1", s.Gate));
            Assert.All(seatsByRow["C"], s => Assert.Equal("2", s.Gate));
            Assert.All(seatsByRow["D"], s => Assert.Equal("2", s.Gate));
            Assert.All(seatsByRow["E"], s => Assert.Equal("3", s.Gate));
        }

        [Theory]
        [InlineData(0, "A")]
        [InlineData(25, "Z")]
        [InlineData(26, "AA")]
        [InlineData(27, "AB")]
        [InlineData(51, "AZ")]
        [InlineData(52, "BA")]
        [InlineData(701, "ZZ")]
        [InlineData(702, "AAA")]
        public void GetRowLabel_ShouldReturnCorrectLabel_WhenZeroBasedIndex(int index, string expectedLabel)
        {
            // Act
            var result = _generator.GetRowLabel(index);

            // Assert
            Assert.Equal(expectedLabel, result);
        }
    }
}
