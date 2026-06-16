namespace InvestAI.Models
{
    public class RailwayData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Source { get; set; } = "OSM Overpass API";
    }
}