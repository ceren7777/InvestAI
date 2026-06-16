namespace InvestAI.Models
{
    public class DistrictAirportDistance
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string AirportName { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
    }
}