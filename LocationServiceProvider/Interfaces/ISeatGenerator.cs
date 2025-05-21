namespace LocationServiceProvider.Interfaces
{
    public interface ISeatGenerator
    {
        List<LocationSeatCreate> GenerateSeats(int seats, int rows, int gates);
    }
}
    

