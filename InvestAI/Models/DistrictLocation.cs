namespace InvestAI.Models
{
    public class DistrictLocation
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Source { get; set; } = "OSM Overpass API";
    }
}