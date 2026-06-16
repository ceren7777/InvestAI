namespace InvestAI.Models
{
    public class GoogleRouteDistance
    {
        public int Id { get; set; }

        public string OriginDistrict { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string DestinationType { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }

        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;

        public string Source { get; set; } = "Google Maps Distance Matrix API";
    }
}