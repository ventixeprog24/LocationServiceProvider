using LocationServiceProvider.Interfaces;

namespace LocationServiceProvider.Helpers
{
    public class SeatGenerator : ISeatGenerator
    {
        public List<LocationSeatCreate> GenerateSeats(int seatCount, int rowCount, int gateCount)
        {
            if (!HasValidFields(seatCount, rowCount, gateCount))
                return [];

            int seatPerRow = seatCount / rowCount;
            int extraSeats = seatCount % rowCount; // Tagit hjälp av AI för hanteringen av extraSeats
            int rowsPerGate = CalculateRowsPerGate(rowCount, gateCount);

            return CreateSeatsWithGates(rowCount, seatPerRow, extraSeats, rowsPerGate);
        }

        public bool HasValidFields(int seatCount, int rowCount, int gateCount)
        {
            if (seatCount <= 0 || rowCount <= 0 || gateCount <= 0)
                return false;
            return true;
        }

        public int CalculateRowsPerGate(int rowCount, int gateCount)
        {
            return (rowCount + gateCount - 1) / gateCount;
        }

        public List<LocationSeatCreate> CreateSeatsWithGates(int rowCount, int seatPerRow, int extraSeats, int rowsPerGate)
        {
            var seats = new List<LocationSeatCreate>();

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string rowLabel = GetRowLabel(rowIndex);
                int seatsInRow = seatPerRow + (rowIndex < extraSeats ? 1 : 0); // Tagit hjälp av AI för hanteringen av extraSeats

                int gateIndex = rowIndex / rowsPerGate; 
                string gateLabel = $"{gateIndex + 1}".ToString();

                for (int seatNumber = 1; seatNumber <= seatsInRow; seatNumber++)
                {
                    seats.Add(new LocationSeatCreate
                    {
                        SeatNumber = seatNumber.ToString(),
                        Row = rowLabel,
                        Gate = gateLabel
                    });
                }
            }

            return seats;
        }

        // Tagit hjälp av AI för att hantera namngivning om fler än 26 rader.
        public string GetRowLabel(int index)
        {
            string label = string.Empty;
            index++;

            while (index > 0)
            {
                index--;
                label = (char)('A' + (index % 26)) + label;
                index /= 26;
            }

            return label;
        }
    }
}
    

