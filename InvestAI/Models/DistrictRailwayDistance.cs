namespace InvestAI.Models
{
    public class DistrictRailwayDistance
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string RailwayName { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
    }
}