namespace InvestAI.Models
{
    public class DistrictPortDistance
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string PortName { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
    }
}